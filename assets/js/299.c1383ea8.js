"use strict";(self.webpackChunkone_click_inventory=self.webpackChunkone_click_inventory||[]).push([[299],{7458:(e,t,a)=>{a.d(t,{A:()=>f});var s=a(758),n=a(3526),l=a(6510),r=a(9081),i=a(732),o=a(1853),c=a(5557),m=a(229);function d(e){const{pathname:t}=(0,c.zy)();return(0,s.useMemo)((()=>e.filter((e=>function(e,t){return!(e.unlisted&&!(0,m.ys)(e.permalink,t))}(e,t)))),[e,t])}const u={sidebar:"sidebar_yFf0",sidebarItemTitle:"sidebarItemTitle_cngB",sidebarItemList:"sidebarItemList_GBeA",sidebarItem:"sidebarItem_TnRj",sidebarItemLink:"sidebarItemLink_aegN",sidebarItemLinkActive:"sidebarItemLinkActive_c7Wd"};var g=a(6070);function h(e){let{sidebar:t}=e;const a=d(t.items);return(0,g.jsx)("aside",{className:"col col--3",children:(0,g.jsxs)("nav",{className:(0,n.A)(u.sidebar,"thin-scrollbar"),"aria-label":(0,o.T)({id:"theme.blog.sidebar.navAriaLabel",message:"Blog recent posts navigation",description:"The ARIA label for recent posts in the blog sidebar"}),children:[(0,g.jsx)("div",{className:(0,n.A)(u.sidebarItemTitle,"margin-bottom--md"),children:t.title}),(0,g.jsx)("ul",{className:(0,n.A)(u.sidebarItemList,"clean-list"),children:a.map((e=>(0,g.jsx)("li",{className:u.sidebarItem,children:(0,g.jsx)(i.A,{isNavLink:!0,to:e.permalink,className:u.sidebarItemLink,activeClassName:u.sidebarItemLinkActive,children:e.title})},e.permalink)))})]})})}var x=a(8060);function j(e){let{sidebar:t}=e;const a=d(t.items);return(0,g.jsx)("ul",{className:"menu__list",children:a.map((e=>(0,g.jsx)("li",{className:"menu__list-item",children:(0,g.jsx)(i.A,{isNavLink:!0,to:e.permalink,className:"menu__link",activeClassName:"menu__link--active",children:e.title})},e.permalink)))})}function b(e){return(0,g.jsx)(x.GX,{component:j,props:e})}function p(e){let{sidebar:t}=e;const a=(0,r.l)();return t?.items.length?"mobile"===a?(0,g.jsx)(b,{sidebar:t}):(0,g.jsx)(h,{sidebar:t}):null}function f(e){const{sidebar:t,toc:a,children:s,...r}=e,i=t&&t.items.length>0;return(0,g.jsx)(l.A,{...r,children:(0,g.jsx)("div",{className:"container margin-vert--lg",children:(0,g.jsxs)("div",{className:"row",children:[(0,g.jsx)(p,{sidebar:t}),(0,g.jsx)("main",{className:(0,n.A)("col",{"col--7":i,"col--9 col--offset-1":!i}),children:s}),a&&(0,g.jsx)("div",{className:"col col--2",children:a})]})})})}},3883:(e,t,a)=>{a.d(t,{A:()=>R});a(758);var s=a(3526),n=a(2607),l=a(6070);function r(e){let{children:t,className:a}=e;return(0,l.jsx)("article",{className:a,children:t})}var i=a(732);const o={title:"title__6Jm"};function c(e){let{className:t}=e;const{metadata:a,isBlogPostPage:r}=(0,n.e)(),{permalink:c,title:m}=a,d=r?"h1":"h2";return(0,l.jsx)(d,{className:(0,s.A)(o.title,t),children:r?m:(0,l.jsx)(i.A,{to:c,children:m})})}var m=a(1853),d=a(5381),u=a(2518);const g={container:"container_yLvF"};function h(e){let{readingTime:t}=e;const a=function(){const{selectMessage:e}=(0,d.W)();return t=>{const a=Math.ceil(t);return e(a,(0,m.T)({id:"theme.blog.post.readingTime.plurals",description:'Pluralized label for "{readingTime} min read". Use as much plural forms (separated by "|") as your language support (see https://www.unicode.org/cldr/cldr-aux/charts/34/supplemental/language_plural_rules.html)',message:"One min read|{readingTime} min read"},{readingTime:a}))}}();return(0,l.jsx)(l.Fragment,{children:a(t)})}function x(e){let{date:t,formattedDate:a}=e;return(0,l.jsx)("time",{dateTime:t,children:a})}function j(){return(0,l.jsx)(l.Fragment,{children:" \xb7 "})}function b(e){let{className:t}=e;const{metadata:a}=(0,n.e)(),{date:r,readingTime:i}=a,o=(0,u.i)({day:"numeric",month:"long",year:"numeric",timeZone:"UTC"});return(0,l.jsxs)("div",{className:(0,s.A)(g.container,"margin-vert--md",t),children:[(0,l.jsx)(x,{date:r,formattedDate:(c=r,o.format(new Date(c)))}),void 0!==i&&(0,l.jsxs)(l.Fragment,{children:[(0,l.jsx)(j,{}),(0,l.jsx)(h,{readingTime:i})]})]});var c}function p(e){return e.href?(0,l.jsx)(i.A,{...e}):(0,l.jsx)(l.Fragment,{children:e.children})}function f(e){let{author:t,className:a}=e;const{name:n,title:r,url:i,imageURL:o,email:c}=t,m=i||c&&`mailto:${c}`||void 0;return(0,l.jsxs)("div",{className:(0,s.A)("avatar margin-bottom--sm",a),children:[o&&(0,l.jsx)(p,{href:m,className:"avatar__photo-link",children:(0,l.jsx)("img",{className:"avatar__photo",src:o,alt:n})}),n&&(0,l.jsxs)("div",{className:"avatar__intro",children:[(0,l.jsx)("div",{className:"avatar__name",children:(0,l.jsx)(p,{href:m,children:(0,l.jsx)("span",{children:n})})}),r&&(0,l.jsx)("small",{className:"avatar__subtitle",children:r})]})]})}const v={authorCol:"authorCol_lH2a",imageOnlyAuthorRow:"imageOnlyAuthorRow_sLSu",imageOnlyAuthorCol:"imageOnlyAuthorCol_bPnz"};function A(e){let{className:t}=e;const{metadata:{authors:a},assets:r}=(0,n.e)();if(0===a.length)return null;const i=a.every((e=>{let{name:t}=e;return!t}));return(0,l.jsx)("div",{className:(0,s.A)("margin-top--md margin-bottom--sm",i?v.imageOnlyAuthorRow:"row",t),children:a.map(((e,t)=>(0,l.jsx)("div",{className:(0,s.A)(!i&&"col col--6",i?v.imageOnlyAuthorCol:v.authorCol),children:(0,l.jsx)(f,{author:{...e,imageURL:r.authorsImageUrls[t]??e.imageURL}})},t)))})}function N(){return(0,l.jsxs)("header",{children:[(0,l.jsx)(c,{}),(0,l.jsx)(b,{}),(0,l.jsx)(A,{})]})}var _=a(5756),k=a(1067);function P(e){let{children:t,className:a}=e;const{isBlogPostPage:r}=(0,n.e)();return(0,l.jsx)("div",{id:r?_.blogPostContainerID:void 0,className:(0,s.A)("markdown",a),children:(0,l.jsx)(k.A,{children:t})})}var T=a(7059),w=a(6494),L=a(2593);function y(){return(0,l.jsx)("b",{children:(0,l.jsx)(m.A,{id:"theme.blog.post.readMore",description:"The label used in blog post item excerpts to link to full blog posts",children:"Read More"})})}function I(e){const{blogPostTitle:t,...a}=e;return(0,l.jsx)(i.A,{"aria-label":(0,m.T)({message:"Read more about {title}",id:"theme.blog.post.readMoreLabel",description:"The ARIA label for the link to full blog posts from excerpts"},{title:t}),...a,children:(0,l.jsx)(y,{})})}function C(){const{metadata:e,isBlogPostPage:t}=(0,n.e)(),{tags:a,title:r,editUrl:i,hasTruncateMarker:o,lastUpdatedBy:c,lastUpdatedAt:m}=e,d=!t&&o,u=a.length>0;if(!(u||d||i))return null;if(t){const e=!!(i||m||c);return(0,l.jsxs)("footer",{className:"docusaurus-mt-lg",children:[u&&(0,l.jsx)("div",{className:(0,s.A)("row","margin-top--sm",T.G.blog.blogFooterEditMetaRow),children:(0,l.jsx)("div",{className:"col",children:(0,l.jsx)(L.A,{tags:a})})}),e&&(0,l.jsx)(w.A,{className:(0,s.A)("margin-top--sm",T.G.blog.blogFooterEditMetaRow),editUrl:i,lastUpdatedAt:m,lastUpdatedBy:c})]})}return(0,l.jsxs)("footer",{className:"row docusaurus-mt-lg",children:[u&&(0,l.jsx)("div",{className:(0,s.A)("col",{"col--9":d}),children:(0,l.jsx)(L.A,{tags:a})}),d&&(0,l.jsx)("div",{className:(0,s.A)("col text--right",{"col--3":u}),children:(0,l.jsx)(I,{blogPostTitle:r,to:e.permalink})})]})}function R(e){let{children:t,className:a}=e;const i=function(){const{isBlogPostPage:e}=(0,n.e)();return e?void 0:"margin-bottom--xl"}();return(0,l.jsxs)(r,{className:(0,s.A)(i,a),children:[(0,l.jsx)(N,{}),(0,l.jsx)(P,{children:t}),(0,l.jsx)(C,{})]})}},3516:(e,t,a)=>{a.d(t,{A:()=>r});a(758);var s=a(3526),n=a(732),l=a(6070);function r(e){const{permalink:t,title:a,subLabel:r,isNext:i}=e;return(0,l.jsxs)(n.A,{className:(0,s.A)("pagination-nav__link",i?"pagination-nav__link--next":"pagination-nav__link--prev"),to:t,children:[r&&(0,l.jsx)("div",{className:"pagination-nav__sublabel",children:r}),(0,l.jsx)("div",{className:"pagination-nav__label",children:a})]})}},717:(e,t,a)=>{a.d(t,{A:()=>i});a(758);var s=a(3526),n=a(732);const l={tag:"tag_kbZO",tagRegular:"tagRegular_q7Tm",tagWithCount:"tagWithCount_ZwAW"};var r=a(6070);function i(e){let{permalink:t,label:a,count:i,description:o}=e;return(0,r.jsxs)(n.A,{href:t,title:o,className:(0,s.A)(l.tag,i?l.tagWithCount:l.tagRegular),children:[a,i&&(0,r.jsx)("span",{children:i})]})}},2593:(e,t,a)=>{a.d(t,{A:()=>o});a(758);var s=a(3526),n=a(1853),l=a(717);const r={tags:"tags_KGbB",tag:"tag_c1zK"};var i=a(6070);function o(e){let{tags:t}=e;return(0,i.jsxs)(i.Fragment,{children:[(0,i.jsx)("b",{children:(0,i.jsx)(n.A,{id:"theme.tags.tagsListLabel",description:"The label alongside a tag list",children:"Tags:"})}),(0,i.jsx)("ul",{className:(0,s.A)(r.tags,"padding--none","margin-left--sm"),children:t.map((e=>(0,i.jsx)("li",{className:r.tag,children:(0,i.jsx)(l.A,{...e})},e.permalink)))})]})}},2607:(e,t,a)=>{a.d(t,{e:()=>o,i:()=>i});var s=a(758),n=a(888),l=a(6070);const r=s.createContext(null);function i(e){let{children:t,content:a,isBlogPostPage:n=!1}=e;const i=function(e){let{content:t,isBlogPostPage:a}=e;return(0,s.useMemo)((()=>({metadata:t.metadata,frontMatter:t.frontMatter,assets:t.assets,toc:t.toc,isBlogPostPage:a})),[t,a])}({content:a,isBlogPostPage:n});return(0,l.jsx)(r.Provider,{value:i,children:t})}function o(){const e=(0,s.useContext)(r);if(null===e)throw new n.dV("BlogPostProvider");return e}},5381:(e,t,a)=>{a.d(t,{W:()=>c});var s=a(758),n=a(1988);const l=["zero","one","two","few","many","other"];function r(e){return l.filter((t=>e.includes(t)))}const i={locale:"en",pluralForms:r(["one","other"]),select:e=>1===e?"one":"other"};function o(){const{i18n:{currentLocale:e}}=(0,n.A)();return(0,s.useMemo)((()=>{try{return function(e){const t=new Intl.PluralRules(e);return{locale:e,pluralForms:r(t.resolvedOptions().pluralCategories),select:e=>t.select(e)}}(e)}catch(t){return console.error(`Failed to use Intl.PluralRules for locale "${e}".\nDocusaurus will fallback to the default (English) implementation.\nError: ${t.message}\n`),i}}),[e])}function c(){const e=o();return{selectMessage:(t,a)=>function(e,t,a){const s=e.split("|");if(1===s.length)return s[0];s.length>a.pluralForms.length&&console.error(`For locale=${a.locale}, a maximum of ${a.pluralForms.length} plural forms are expected (${a.pluralForms.join(",")}), but the message contains ${s.length}: ${e}`);const n=a.select(t),l=a.pluralForms.indexOf(n);return s[Math.min(l,s.length-1)]}(a,t,e)}}}}]);