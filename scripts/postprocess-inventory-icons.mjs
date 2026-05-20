import fs from "node:fs";
import crypto from "node:crypto";
import path from "node:path";
import zlib from "node:zlib";

const defaultSheetItems = [
  ["Hat", 0, 0],
  ["Gear", 0, 1],
  ["Shirt", 0, 2],
  ["Pants", 0, 3],
  ["Skirt", 1, 0],
  ["Panties", 1, 1],
  ["Bra", 1, 2],
  ["R18", 1, 3],
  ["Warning", 2, 0],
  ["Shoes", 2, 1],
  ["Socks", 2, 2],
  ["Hairpin", 2, 3],
  ["Gloves", 3, 0],
  ["Star", 3, 1],
];

const crcTable = new Uint32Array(256).map((_, n) => {
  let c = n;

  for (let k = 0; k < 8; k++) {
    c = c & 1 ? 0xedb88320 ^ (c >>> 1) : c >>> 1;
  }

  return c >>> 0;
});

const options = parseArgs(process.argv.slice(2));

if (options.help || !options.source) {
  printHelp();
  process.exit(options.help ? 0 : 1);
}

const sourcePath = path.resolve(options.source);
const outputDir = path.resolve(options.out ?? "Icons/DefaultInventory");
const size = parsePositiveInt(options.size ?? "256", "size");
const maxSide = parsePositiveInt(options["max-side"] ?? "216", "max-side");
const sheetColumns = parsePositiveInt(options.cols ?? "4", "cols");
const sheetRows = parsePositiveInt(options.rows ?? "4", "rows");
const mode = options.mode ?? (options.name ? "single" : "sheet");

fs.mkdirSync(outputDir, { recursive: true });

const source = decodePng(sourcePath);
const written =
  mode === "single"
    ? writeSingleIcon(source, outputDir, options.name ?? "Icon", size, maxSide)
    : writeSheetIcons(
        source,
        outputDir,
        parseItems(options.items),
        sheetColumns,
        sheetRows,
        size,
        maxSide,
      );

for (const filePath of written) {
  console.log(filePath);
}

function parseArgs(args) {
  const parsed = {};

  for (let index = 0; index < args.length; index++) {
    const arg = args[index];

    if (arg === "--help" || arg === "-h") {
      parsed.help = true;
      continue;
    }

    if (!arg.startsWith("--")) {
      throw new Error(`Unexpected argument: ${arg}`);
    }

    const key = arg.slice(2);
    const next = args[index + 1];

    if (!next || next.startsWith("--")) {
      parsed[key] = "true";
      continue;
    }

    parsed[key] = next;
    index++;
  }

  return parsed;
}

function printHelp() {
  console.log(`Usage:
  node scripts/postprocess-inventory-icons.mjs --source <png> [--out Icons/DefaultInventory]

Sheet mode, default:
  node scripts/postprocess-inventory-icons.mjs --source generated-sheet.png

Single icon mode:
  node scripts/postprocess-inventory-icons.mjs --mode single --name OnePiece --source generated-onepiece.png

Custom sheet items:
  node scripts/postprocess-inventory-icons.mjs --source sheet.png --items "OnePiece:0:0,Dress:0:1"

Options:
  --mode sheet|single
  --name <file-name-without-extension>  Used by single mode.
  --out <directory>                    Default: Icons/DefaultInventory
  --size <px>                          Default: 256
  --max-side <px>                      Default: 216
  --cols <count>                       Default: 4
  --rows <count>                       Default: 4
`);
}

function parsePositiveInt(value, name) {
  const parsed = Number.parseInt(value, 10);

  if (!Number.isFinite(parsed) || parsed <= 0) {
    throw new Error(`--${name} must be a positive integer.`);
  }

  return parsed;
}

function parseItems(value) {
  if (!value) {
    return defaultSheetItems;
  }

  return value.split(",").map((entry) => {
    const [name, row, col] = entry.split(":");

    if (!name || row === undefined || col === undefined) {
      throw new Error(`Invalid item entry: ${entry}. Use Name:row:col.`);
    }

    return [
      name,
      parseNonNegativeInt(row, "items row"),
      parseNonNegativeInt(col, "items col"),
    ];
  });
}

function parseNonNegativeInt(value, name) {
  const parsed = Number.parseInt(value, 10);

  if (!Number.isFinite(parsed) || parsed < 0) {
    throw new Error(`--${name} must be a non-negative integer.`);
  }

  return parsed;
}

function writeSingleIcon(source, outputDir, name, size, maxSide) {
  const icon = processIcon(source, size, maxSide);
  const outputPath = path.join(outputDir, `${name}.png`);
  writeIcon(outputPath, icon);
  return [outputPath];
}

function writeSheetIcons(source, outputDir, items, columns, rows, size, maxSide) {
  const written = [];

  for (const [name, row, column] of items) {
    const left = Math.round((column * source.width) / columns);
    const top = Math.round((row * source.height) / rows);
    const right = Math.round(((column + 1) * source.width) / columns);
    const bottom = Math.round(((row + 1) * source.height) / rows);
    const cell = crop(source, left, top, right - left, bottom - top);
    const icon = processIcon(cell, size, maxSide);
    const outputPath = path.join(outputDir, `${name}.png`);

    writeIcon(outputPath, icon);
    written.push(outputPath);
  }

  return written;
}

function writeIcon(outputPath, icon) {
  fs.writeFileSync(outputPath, encodePng(icon.width, icon.height, icon.pixels));
  ensureTextureMeta(outputPath);
}

function ensureTextureMeta(texturePath) {
  const metaPath = `${texturePath}.meta`;

  if (!fs.existsSync(metaPath)) {
    fs.writeFileSync(metaPath, createTextureMeta());
    return;
  }

  const original = fs.readFileSync(metaPath, "utf8");
  const updated = ensureMetaProperty(
    ensureMetaProperty(original, "alphaUsage", "1"),
    "alphaIsTransparency",
    "1",
  );

  if (updated !== original) {
    fs.writeFileSync(metaPath, updated);
  }
}

function ensureMetaProperty(content, key, value) {
  const propertyPattern = new RegExp(`(^\\s*${key}:\\s*)\\S+`, "m");

  if (propertyPattern.test(content)) {
    return content.replace(propertyPattern, `$1${value}`);
  }

  const textureTypePattern = /^(\s*textureType:\s*\S+)/m;

  if (textureTypePattern.test(content)) {
    return content.replace(textureTypePattern, `${key}: ${value}\n$1`);
  }

  return `${content.replace(/\s*$/, "")}\n  ${key}: ${value}\n`;
}

function createTextureMeta() {
  return `fileFormatVersion: 2
guid: ${crypto.randomBytes(16).toString("hex")}
TextureImporter:
  internalIDToNameTable: []
  externalObjects: {}
  serializedVersion: 12
  mipmaps:
    mipMapMode: 0
    enableMipMap: 1
    sRGBTexture: 1
    linearTexture: 0
    fadeOut: 0
    borderMipMap: 0
    mipMapsPreserveCoverage: 0
    alphaTestReferenceValue: 0.5
    mipMapFadeDistanceStart: 1
    mipMapFadeDistanceEnd: 3
  bumpmap:
    convertToNormalMap: 0
    externalNormalMap: 0
    heightScale: 0.25
    normalMapFilter: 0
    flipGreenChannel: 0
  isReadable: 0
  streamingMipmaps: 0
  streamingMipmapsPriority: 0
  vTOnly: 0
  ignoreMipmapLimit: 0
  grayScaleToAlpha: 0
  generateCubemap: 6
  cubemapConvolution: 0
  seamlessCubemap: 0
  textureFormat: 1
  maxTextureSize: 2048
  textureSettings:
    serializedVersion: 2
    filterMode: 1
    aniso: 1
    mipBias: 0
    wrapU: 0
    wrapV: 0
    wrapW: 0
  nPOTScale: 1
  lightmap: 0
  compressionQuality: 50
  spriteMode: 0
  spriteExtrude: 1
  spriteMeshType: 1
  alignment: 0
  spritePivot: {x: 0.5, y: 0.5}
  spritePixelsToUnits: 100
  spriteBorder: {x: 0, y: 0, z: 0, w: 0}
  spriteGenerateFallbackPhysicsShape: 1
  alphaUsage: 1
  alphaIsTransparency: 1
  spriteTessellationDetail: -1
  textureType: 0
  textureShape: 1
  singleChannelComponent: 0
  flipbookRows: 1
  flipbookColumns: 1
  maxTextureSizeSet: 0
  compressionQualitySet: 0
  textureFormatSet: 0
  ignorePngGamma: 0
  applyGammaDecoding: 0
  swizzle: 50462976
  cookieLightType: 0
  platformSettings:
  - serializedVersion: 3
    buildTarget: DefaultTexturePlatform
    maxTextureSize: 256
    resizeAlgorithm: 0
    textureFormat: -1
    textureCompression: 1
    compressionQuality: 50
    crunchedCompression: 0
    allowsAlphaSplitting: 0
    overridden: 0
    ignorePlatformSupport: 0
    androidETC2FallbackOverride: 0
    forceMaximumCompressionQuality_BC6H_BC7: 0
  - serializedVersion: 3
    buildTarget: Standalone
    maxTextureSize: 2048
    resizeAlgorithm: 0
    textureFormat: -1
    textureCompression: 1
    compressionQuality: 50
    crunchedCompression: 0
    allowsAlphaSplitting: 0
    overridden: 0
    ignorePlatformSupport: 0
    androidETC2FallbackOverride: 0
    forceMaximumCompressionQuality_BC6H_BC7: 0
  - serializedVersion: 3
    buildTarget: Android
    maxTextureSize: 2048
    resizeAlgorithm: 0
    textureFormat: -1
    textureCompression: 1
    compressionQuality: 50
    crunchedCompression: 0
    allowsAlphaSplitting: 0
    overridden: 0
    ignorePlatformSupport: 0
    androidETC2FallbackOverride: 0
    forceMaximumCompressionQuality_BC6H_BC7: 0
  spriteSheet:
    serializedVersion: 2
    sprites: []
    outline: []
    physicsShape: []
    bones: []
    spriteID: 
    internalID: 0
    vertices: []
    indices: 
    edges: []
    weights: []
    secondaryTextures: []
    nameFileIdTable: {}
  mipmapLimitGroupName: 
  pSDRemoveMatte: 0
  userData: 
  assetBundleName: 
  assetBundleVariant: 
`;
}

function processIcon(source, size, maxSide) {
  const mask = dilateAlphaTiny(alphaMaskFromLight(source));
  return resizeAlphaToGradientCanvas(mask, size, maxSide);
}

function alphaMaskFromLight(image) {
  const pixels = new Uint8ClampedArray(image.width * image.height * 4);

  for (let y = 0; y < image.height; y++) {
    for (let x = 0; x < image.width; x++) {
      const [r, g, b] = getPixel(image, x, y);
      const light = Math.max(r, g, b);
      const alpha = Math.round(smoothstep(16, 118, light) * 255);

      setPixel(pixels, image.width, x, y, 255, 255, 255, alpha);
    }
  }

  return { width: image.width, height: image.height, pixels };
}

function dilateAlphaTiny(image) {
  const pixels = new Uint8ClampedArray(image.pixels);

  for (let y = 1; y < image.height - 1; y++) {
    for (let x = 1; x < image.width - 1; x++) {
      const alphaIndex = (y * image.width + x) * 4 + 3;

      if (image.pixels[alphaIndex] >= 24) {
        continue;
      }

      let maxAlpha = 0;

      for (let offsetY = -1; offsetY <= 1; offsetY++) {
        for (let offsetX = -1; offsetX <= 1; offsetX++) {
          const neighborIndex =
            ((y + offsetY) * image.width + x + offsetX) * 4 + 3;
          maxAlpha = Math.max(maxAlpha, image.pixels[neighborIndex]);
        }
      }

      if (maxAlpha > 180) {
        pixels[alphaIndex] = Math.max(pixels[alphaIndex], 28);
      }
    }
  }

  return { width: image.width, height: image.height, pixels };
}

function resizeAlphaToGradientCanvas(mask, size, maxSide) {
  const maskBounds = bounds(mask);
  const content = crop(
    mask,
    maskBounds.x,
    maskBounds.y,
    maskBounds.width,
    maskBounds.height,
  );
  const scale = Math.min(maxSide / content.width, maxSide / content.height);
  const drawWidth = Math.round(content.width * scale);
  const drawHeight = Math.round(content.height * scale);
  const offsetX = Math.round((size - drawWidth) / 2);
  const offsetY = Math.round((size - drawHeight) / 2);
  const pixels = new Uint8ClampedArray(size * size * 4);

  for (let y = 0; y < drawHeight; y++) {
    for (let x = 0; x < drawWidth; x++) {
      const sourceX = (x + 0.5) / scale - 0.5;
      const sourceY = (y + 0.5) / scale - 0.5;
      const alpha = sampleAlphaBilinear(content, sourceX, sourceY);

      if (alpha <= 0) {
        continue;
      }

      const gradientPosition = Math.max(
        0,
        Math.min(1, y / Math.max(1, drawHeight - 1)),
      );
      const base = Math.round(255 - 27 * gradientPosition);
      const highlightOffset = (gradientPosition - 0.18) / 0.28;
      const topHighlight = Math.round(
        8 * Math.exp(-(highlightOffset ** 2)),
      );
      const value = Math.max(222, Math.min(255, base + topHighlight));

      setPixel(
        pixels,
        size,
        offsetX + x,
        offsetY + y,
        value,
        value,
        value,
        alpha,
      );
    }
  }

  return { width: size, height: size, pixels };
}

function bounds(image) {
  let minX = image.width;
  let minY = image.height;
  let maxX = -1;
  let maxY = -1;

  for (let y = 0; y < image.height; y++) {
    for (let x = 0; x < image.width; x++) {
      const alpha = image.pixels[(y * image.width + x) * 4 + 3];

      if (alpha <= 10) {
        continue;
      }

      minX = Math.min(minX, x);
      minY = Math.min(minY, y);
      maxX = Math.max(maxX, x);
      maxY = Math.max(maxY, y);
    }
  }

  if (maxX < 0) {
    return { x: 0, y: 0, width: image.width, height: image.height };
  }

  const padding = 3;
  minX = Math.max(0, minX - padding);
  minY = Math.max(0, minY - padding);
  maxX = Math.min(image.width - 1, maxX + padding);
  maxY = Math.min(image.height - 1, maxY + padding);

  return {
    x: minX,
    y: minY,
    width: maxX - minX + 1,
    height: maxY - minY + 1,
  };
}

function smoothstep(edge0, edge1, x) {
  const t = Math.max(0, Math.min(1, (x - edge0) / (edge1 - edge0)));
  return t * t * (3 - 2 * t);
}

function sampleAlphaBilinear(image, x, y) {
  const x0 = Math.max(0, Math.min(image.width - 1, Math.floor(x)));
  const y0 = Math.max(0, Math.min(image.height - 1, Math.floor(y)));
  const x1 = Math.max(0, Math.min(image.width - 1, x0 + 1));
  const y1 = Math.max(0, Math.min(image.height - 1, y0 + 1));
  const tx = x - x0;
  const ty = y - y0;
  const a00 = image.pixels[(y0 * image.width + x0) * 4 + 3];
  const a10 = image.pixels[(y0 * image.width + x1) * 4 + 3];
  const a01 = image.pixels[(y1 * image.width + x0) * 4 + 3];
  const a11 = image.pixels[(y1 * image.width + x1) * 4 + 3];

  return Math.round(
    (a00 * (1 - tx) + a10 * tx) * (1 - ty) +
      (a01 * (1 - tx) + a11 * tx) * ty,
  );
}

function crop(image, x, y, width, height) {
  const pixels = new Uint8ClampedArray(width * height * 4);

  for (let targetY = 0; targetY < height; targetY++) {
    for (let targetX = 0; targetX < width; targetX++) {
      const [r, g, b, a] = getPixel(image, x + targetX, y + targetY);
      setPixel(pixels, width, targetX, targetY, r, g, b, a);
    }
  }

  return { width, height, pixels };
}

function getPixel(image, x, y) {
  const index = (y * image.width + x) * 4;
  return [
    image.pixels[index],
    image.pixels[index + 1],
    image.pixels[index + 2],
    image.pixels[index + 3],
  ];
}

function setPixel(pixels, width, x, y, r, g, b, a) {
  const index = (y * width + x) * 4;
  pixels[index] = r;
  pixels[index + 1] = g;
  pixels[index + 2] = b;
  pixels[index + 3] = a;
}

function decodePng(filePath) {
  const buffer = fs.readFileSync(filePath);
  const signature = Buffer.from([137, 80, 78, 71, 13, 10, 26, 10]);

  if (!buffer.subarray(0, 8).equals(signature)) {
    throw new Error(`${filePath} is not a PNG file.`);
  }

  let offset = 8;
  let width = 0;
  let height = 0;
  let bitDepth = 0;
  let colorType = 0;
  const idatChunks = [];

  while (offset < buffer.length) {
    const length = buffer.readUInt32BE(offset);
    offset += 4;
    const type = buffer.toString("ascii", offset, offset + 4);
    offset += 4;
    const data = buffer.subarray(offset, offset + length);
    offset += length + 4;

    if (type === "IHDR") {
      width = data.readUInt32BE(0);
      height = data.readUInt32BE(4);
      bitDepth = data[8];
      colorType = data[9];
    } else if (type === "IDAT") {
      idatChunks.push(data);
    } else if (type === "IEND") {
      break;
    }
  }

  if (bitDepth !== 8 || ![2, 6].includes(colorType)) {
    throw new Error(
      `Unsupported PNG format: bitDepth=${bitDepth}, colorType=${colorType}.`,
    );
  }

  const channels = colorType === 6 ? 4 : 3;
  const stride = width * channels;
  const raw = zlib.inflateSync(Buffer.concat(idatChunks));
  const pixels = new Uint8ClampedArray(width * height * 4);
  let rawOffset = 0;
  let previousRow = new Uint8Array(stride);

  for (let y = 0; y < height; y++) {
    const filter = raw[rawOffset++];
    const row = new Uint8Array(stride);

    for (let x = 0; x < stride; x++) {
      const value = raw[rawOffset++];
      const left = x >= channels ? row[x - channels] : 0;
      const up = previousRow[x] || 0;
      const upLeft = x >= channels ? previousRow[x - channels] : 0;

      if (filter === 0) {
        row[x] = value;
      } else if (filter === 1) {
        row[x] = (value + left) & 255;
      } else if (filter === 2) {
        row[x] = (value + up) & 255;
      } else if (filter === 3) {
        row[x] = (value + Math.floor((left + up) / 2)) & 255;
      } else if (filter === 4) {
        row[x] = (value + paeth(left, up, upLeft)) & 255;
      } else {
        throw new Error(`Unsupported PNG filter: ${filter}.`);
      }
    }

    for (let x = 0; x < width; x++) {
      const sourceIndex = x * channels;
      const targetIndex = (y * width + x) * 4;
      pixels[targetIndex] = row[sourceIndex];
      pixels[targetIndex + 1] = row[sourceIndex + 1];
      pixels[targetIndex + 2] = row[sourceIndex + 2];
      pixels[targetIndex + 3] = channels === 4 ? row[sourceIndex + 3] : 255;
    }

    previousRow = row;
  }

  return { width, height, pixels };
}

function paeth(a, b, c) {
  const p = a + b - c;
  const pa = Math.abs(p - a);
  const pb = Math.abs(p - b);
  const pc = Math.abs(p - c);

  if (pa <= pb && pa <= pc) {
    return a;
  }

  if (pb <= pc) {
    return b;
  }

  return c;
}

function crc32(buffer) {
  let c = 0xffffffff;

  for (const byte of buffer) {
    c = crcTable[(c ^ byte) & 255] ^ (c >>> 8);
  }

  return (c ^ 0xffffffff) >>> 0;
}

function pngChunk(type, data) {
  const typeBuffer = Buffer.from(type, "ascii");
  const output = Buffer.alloc(12 + data.length);
  output.writeUInt32BE(data.length, 0);
  typeBuffer.copy(output, 4);
  data.copy(output, 8);
  output.writeUInt32BE(crc32(Buffer.concat([typeBuffer, data])), 8 + data.length);
  return output;
}

function encodePng(width, height, pixels) {
  const ihdr = Buffer.alloc(13);
  ihdr.writeUInt32BE(width, 0);
  ihdr.writeUInt32BE(height, 4);
  ihdr[8] = 8;
  ihdr[9] = 6;
  ihdr[10] = 0;
  ihdr[11] = 0;
  ihdr[12] = 0;

  const raw = Buffer.alloc((width * 4 + 1) * height);
  let offset = 0;

  for (let y = 0; y < height; y++) {
    raw[offset++] = 0;

    for (let x = 0; x < width * 4; x++) {
      raw[offset++] = pixels[y * width * 4 + x];
    }
  }

  return Buffer.concat([
    Buffer.from([137, 80, 78, 71, 13, 10, 26, 10]),
    pngChunk("IHDR", ihdr),
    pngChunk("IDAT", zlib.deflateSync(raw, { level: 9 })),
    pngChunk("IEND", Buffer.alloc(0)),
  ]);
}
