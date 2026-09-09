//! Licensed to the .NET Foundation under one or more agreements.
//! The .NET Foundation licenses this file to you under the MIT license.

var e=!1;const t=async()=>WebAssembly.validate(new Uint8Array([0,97,115,109,1,0,0,0,1,4,1,96,0,0,3,2,1,0,10,8,1,6,0,6,64,25,11,11])),o=async()=>WebAssembly.validate(new Uint8Array([0,97,115,109,1,0,0,0,1,5,1,96,0,1,123,3,2,1,0,10,15,1,13,0,65,1,253,15,65,2,253,15,253,128,2,11])),n=async()=>WebAssembly.validate(new Uint8Array([0,97,115,109,1,0,0,0,1,5,1,96,0,1,123,3,2,1,0,10,10,1,8,0,65,0,253,15,253,98,11])),r=Symbol.for("wasm promise_control");function i(e,t){let o=null;const n=new Promise((function(n,r){o={isDone:!1,promise:null,resolve:t=>{o.isDone||(o.isDone=!0,n(t),e&&e())},reject:e=>{o.isDone||(o.isDone=!0,r(e),t&&t())}}}));o.promise=n;const i=n;return i[r]=o,{promise:i,promise_control:o}}function s(e){return e[r]}function a(e){e&&function(e){return void 0!==e[r]}(e)||Be(!1,"Promise is not controllable")}const l="__mono_message__",c=["debug","log","trace","warn","info","error"],d="MONO_WASM: ";let u,f,m,g,p,h;function w(e){g=e}function b(e){if(Pe.diagnosticTracing){const t="function"==typeof e?e():e;console.debug(d+t)}}function y(e,...t){console.info(d+e,...t)}function v(e,...t){console.info(e,...t)}function E(e,...t){console.warn(d+e,...t)}function _(e,...t){if(t&&t.length>0&&t[0]&&"object"==typeof t[0]){if(t[0].silent)return;if(t[0].toString)return void console.error(d+e,t[0].toString())}console.error(d+e,...t)}function x(e,t,o){return function(...n){try{let r=n[0];if(void 0===r)r="undefined";else if(null===r)r="null";else if("function"==typeof r)r=r.toString();else if("string"!=typeof r)try{r=JSON.stringify(r)}catch(e){r=r.toString()}t(o?JSON.stringify({method:e,payload:r,arguments:n.slice(1)}):[e+r,...n.slice(1)])}catch(e){m.error(`proxyConsole failed: ${e}`)}}}function j(e,t,o){f=t,g=e,m={...t};const n=`${o}/console`.replace("https://","wss://").replace("http://","ws://");u=new WebSocket(n),u.addEventListener("error",A),u.addEventListener("close",S),function(){for(const e of c)f[e]=x(`console.${e}`,T,!0)}()}function R(e){let t=30;const o=()=>{u?0==u.bufferedAmount||0==t?(e&&v(e),function(){for(const e of c)f[e]=x(`console.${e}`,m.log,!1)}(),u.removeEventListener("error",A),u.removeEventListener("close",S),u.close(1e3,e),u=void 0):(t--,globalThis.setTimeout(o,100)):e&&m&&m.log(e)};o()}function T(e){u&&u.readyState===WebSocket.OPEN?u.send(e):m.log(e)}function A(e){m.error(`[${g}] proxy console websocket error: ${e}`,e)}function S(e){m.debug(`[${g}] proxy console websocket closed: ${e}`,e)}function D(){Pe.preferredIcuAsset=O(Pe.config);let e="invariant"==Pe.config.globalizationMode;if(!e)if(Pe.preferredIcuAsset)Pe.diagnosticTracing&&b("ICU data archive(s) available, disabling invariant mode");else{if("custom"===Pe.config.globalizationMode||"all"===Pe.config.globalizationMode||"sharded"===Pe.config.globalizationMode){const e="invariant globalization mode is inactive and no ICU data archives are available";throw _(`ERROR: ${e}`),new Error(e)}Pe.diagnosticTracing&&b("ICU data archive(s) not available, using invariant globalization mode"),e=!0,Pe.preferredIcuAsset=null}const t="DOTNET_SYSTEM_GLOBALIZATION_INVARIANT",o=Pe.config.environmentVariables;if(void 0===o[t]&&e&&(o[t]="1"),void 0===o.TZ)try{const e=Intl.DateTimeFormat().resolvedOptions().timeZone||null;e&&(o.TZ=e)}catch(e){y("failed to detect timezone, will fallback to UTC")}}function O(e){var t;if((null===(t=e.resources)||void 0===t?void 0:t.icu)&&"invariant"!=e.globalizationMode){const t=e.applicationCulture||(ke?globalThis.navigator&&globalThis.navigator.languages&&globalThis.navigator.languages[0]:Intl.DateTimeFormat().resolvedOptions().locale),o=e.resources.icu;let n=null;if("custom"===e.globalizationMode){if(o.length>=1)return o[0].name}else t&&"all"!==e.globalizationMode?"sharded"===e.globalizationMode&&(n=function(e){const t=e.split("-")[0];return"en"===t||["fr","fr-FR","it","it-IT","de","de-DE","es","es-ES"].includes(e)?"icudt_EFIGS.dat":["zh","ko","ja"].includes(t)?"icudt_CJK.dat":"icudt_no_CJK.dat"}(t)):n="icudt.dat";if(n)for(let e=0;e<o.length;e++){const t=o[e];if(t.virtualPath===n)return t.name}}return e.globalizationMode="invariant",null}(new Date).valueOf();const C=class{constructor(e){this.url=e}toString(){return this.url}};async function k(e,t){try{const o="function"==typeof globalThis.fetch;if(Se){const n=e.startsWith("file://");if(!n&&o)return globalThis.fetch(e,t||{credentials:"same-origin"});p||(h=Ne.require("url"),p=Ne.require("fs")),n&&(e=h.fileURLToPath(e));const r=await p.promises.readFile(e);return{ok:!0,headers:{length:0,get:()=>null},url:e,arrayBuffer:()=>r,json:()=>JSON.parse(r),text:()=>{throw new Error("NotImplementedException")}}}if(o)return globalThis.fetch(e,t||{credentials:"same-origin"});if("function"==typeof read)return{ok:!0,url:e,headers:{length:0,get:()=>null},arrayBuffer:()=>new Uint8Array(read(e,"binary")),json:()=>JSON.parse(read(e,"utf8")),text:()=>read(e,"utf8")}}catch(t){return{ok:!1,url:e,status:500,headers:{length:0,get:()=>null},statusText:"ERR28: "+t,arrayBuffer:()=>{throw t},json:()=>{throw t},text:()=>{throw t}}}throw new Error("No fetch implementation available")}function I(e){return"string"!=typeof e&&Be(!1,"url must be a string"),!M(e)&&0!==e.indexOf("./")&&0!==e.indexOf("../")&&globalThis.URL&&globalThis.document&&globalThis.document.baseURI&&(e=new URL(e,globalThis.document.baseURI).toString()),e}const U=/^[a-zA-Z][a-zA-Z\d+\-.]*?:\/\//,P=/[a-zA-Z]:[\\/]/;function M(e){return Se||Ie?e.startsWith("/")||e.startsWith("\\")||-1!==e.indexOf("///")||P.test(e):U.test(e)}let L,N=0;const $=[],z=[],W=new Map,F={"js-module-threads":!0,"js-module-runtime":!0,"js-module-dotnet":!0,"js-module-native":!0,"js-module-diagnostics":!0},B={...F,"js-module-library-initializer":!0},V={...F,dotnetwasm:!0,heap:!0,manifest:!0},q={...B,manifest:!0},H={...B,dotnetwasm:!0},J={dotnetwasm:!0,symbols:!0},Z={...B,dotnetwasm:!0,symbols:!0},Q={symbols:!0};function G(e){return!("icu"==e.behavior&&e.name!=Pe.preferredIcuAsset)}function K(e,t,o){null!=t||(t=[]),Be(1==t.length,`Expect to have one ${o} asset in resources`);const n=t[0];return n.behavior=o,X(n),e.push(n),n}function X(e){V[e.behavior]&&W.set(e.behavior,e)}function Y(e){Be(V[e],`Unknown single asset behavior ${e}`);const t=W.get(e);if(t&&!t.resolvedUrl)if(t.resolvedUrl=Pe.locateFile(t.name),F[t.behavior]){const e=ge(t);e?("string"!=typeof e&&Be(!1,"loadBootResource response for 'dotnetjs' type should be a URL string"),t.resolvedUrl=e):t.resolvedUrl=ce(t.resolvedUrl,t.behavior)}else if("dotnetwasm"!==t.behavior)throw new Error(`Unknown single asset behavior ${e}`);return t}function ee(e){const t=Y(e);return Be(t,`Single asset for ${e} not found`),t}let te=!1;async function oe(){if(!te){te=!0,Pe.diagnosticTracing&&b("mono_download_assets");try{const e=[],t=[],o=(e,t)=>{!Z[e.behavior]&&G(e)&&Pe.expected_instantiated_assets_count++,!H[e.behavior]&&G(e)&&(Pe.expected_downloaded_assets_count++,t.push(se(e)))};for(const t of $)o(t,e);for(const e of z)o(e,t);Pe.allDownloadsQueued.promise_control.resolve(),Promise.all([...e,...t]).then((()=>{Pe.allDownloadsFinished.promise_control.resolve()})).catch((e=>{throw Pe.err("Error in mono_download_assets: "+e),Xe(1,e),e})),await Pe.runtimeModuleLoaded.promise;const n=async e=>{const t=await e;if(t.buffer){if(!Z[t.behavior]){t.buffer&&"object"==typeof t.buffer||Be(!1,"asset buffer must be array-like or buffer-like or promise of these"),"string"!=typeof t.resolvedUrl&&Be(!1,"resolvedUrl must be string");const e=t.resolvedUrl,o=await t.buffer,n=new Uint8Array(o);pe(t),await Ue.beforeOnRuntimeInitialized.promise,Ue.instantiate_asset(t,e,n)}}else J[t.behavior]?("symbols"===t.behavior&&(await Ue.instantiate_symbols_asset(t),pe(t)),J[t.behavior]&&++Pe.actual_downloaded_assets_count):(t.isOptional||Be(!1,"Expected asset to have the downloaded buffer"),!H[t.behavior]&&G(t)&&Pe.expected_downloaded_assets_count--,!Z[t.behavior]&&G(t)&&Pe.expected_instantiated_assets_count--)},r=[],i=[];for(const t of e)r.push(n(t));for(const e of t)i.push(n(e));Promise.all(r).then((()=>{Ce||Ue.coreAssetsInMemory.promise_control.resolve()})).catch((e=>{throw Pe.err("Error in mono_download_assets: "+e),Xe(1,e),e})),Promise.all(i).then((async()=>{Ce||(await Ue.coreAssetsInMemory.promise,Ue.allAssetsInMemory.promise_control.resolve())})).catch((e=>{throw Pe.err("Error in mono_download_assets: "+e),Xe(1,e),e}))}catch(e){throw Pe.err("Error in mono_download_assets: "+e),e}}}let ne=!1;function re(){if(ne)return;ne=!0;const e=Pe.config,t=[];if(e.assets)for(const t of e.assets)"object"!=typeof t&&Be(!1,`asset must be object, it was ${typeof t} : ${t}`),"string"!=typeof t.behavior&&Be(!1,"asset behavior must be known string"),"string"!=typeof t.name&&Be(!1,"asset name must be string"),t.resolvedUrl&&"string"!=typeof t.resolvedUrl&&Be(!1,"asset resolvedUrl could be string"),t.hash&&"string"!=typeof t.hash&&Be(!1,"asset resolvedUrl could be string"),t.pendingDownload&&"object"!=typeof t.pendingDownload&&Be(!1,"asset pendingDownload could be object"),t.isCore?$.push(t):z.push(t),X(t);else if(e.resources){const o=e.resources;o.wasmNative||Be(!1,"resources.wasmNative must be defined"),o.jsModuleNative||Be(!1,"resources.jsModuleNative must be defined"),o.jsModuleRuntime||Be(!1,"resources.jsModuleRuntime must be defined"),K(z,o.wasmNative,"dotnetwasm"),K(t,o.jsModuleNative,"js-module-native"),K(t,o.jsModuleRuntime,"js-module-runtime"),o.jsModuleDiagnostics&&K(t,o.jsModuleDiagnostics,"js-module-diagnostics");const n=(e,t,o)=>{const n=e;n.behavior=t,o?(n.isCore=!0,$.push(n)):z.push(n)};if(o.coreAssembly)for(let e=0;e<o.coreAssembly.length;e++)n(o.coreAssembly[e],"assembly",!0);if(o.assembly)for(let e=0;e<o.assembly.length;e++)n(o.assembly[e],"assembly",!o.coreAssembly);if(0!=e.debugLevel&&Pe.isDebuggingSupported()){if(o.corePdb)for(let e=0;e<o.corePdb.length;e++)n(o.corePdb[e],"pdb",!0);if(o.pdb)for(let e=0;e<o.pdb.length;e++)n(o.pdb[e],"pdb",!o.corePdb)}if(e.loadAllSatelliteResources&&o.satelliteResources)for(const e in o.satelliteResources)for(let t=0;t<o.satelliteResources[e].length;t++){const r=o.satelliteResources[e][t];r.culture=e,n(r,"resource",!o.coreAssembly)}if(o.coreVfs)for(let e=0;e<o.coreVfs.length;e++)n(o.coreVfs[e],"vfs",!0);if(o.vfs)for(let e=0;e<o.vfs.length;e++)n(o.vfs[e],"vfs",!o.coreVfs);const r=O(e);if(r&&o.icu)for(let e=0;e<o.icu.length;e++){const t=o.icu[e];t.name===r&&n(t,"icu",!1)}if(o.wasmSymbols)for(let e=0;e<o.wasmSymbols.length;e++)n(o.wasmSymbols[e],"symbols",!1)}if(e.appsettings)for(let t=0;t<e.appsettings.length;t++){const o=e.appsettings[t],n=he(o);"appsettings.json"!==n&&n!==`appsettings.${e.applicationEnvironment}.json`||z.push({name:o,behavior:"vfs",cache:"no-cache",useCredentials:!0})}e.assets=[...$,...z,...t]}async function ie(e){const t=await se(e);return await t.pendingDownloadInternal.response,t.buffer}async function se(e){try{return await ae(e)}catch(t){if(!Pe.enableDownloadRetry)throw t;if(Ie||Se)throw t;if(e.pendingDownload&&e.pendingDownloadInternal==e.pendingDownload)throw t;if(e.resolvedUrl&&-1!=e.resolvedUrl.indexOf("file://"))throw t;if(t&&404==t.status)throw t;e.pendingDownloadInternal=void 0,await Pe.allDownloadsQueued.promise;try{return Pe.diagnosticTracing&&b(`Retrying download '${e.name}'`),await ae(e)}catch(t){return e.pendingDownloadInternal=void 0,await new Promise((e=>globalThis.setTimeout(e,100))),Pe.diagnosticTracing&&b(`Retrying download (2) '${e.name}' after delay`),await ae(e)}}}async function ae(e){for(;L;)await L.promise;try{++N,N==Pe.maxParallelDownloads&&(Pe.diagnosticTracing&&b("Throttling further parallel downloads"),L=i());const t=await async function(e){if(e.pendingDownload&&(e.pendingDownloadInternal=e.pendingDownload),e.pendingDownloadInternal&&e.pendingDownloadInternal.response)return e.pendingDownloadInternal.response;if(e.buffer){const t=await e.buffer;return e.resolvedUrl||(e.resolvedUrl="undefined://"+e.name),e.pendingDownloadInternal={url:e.resolvedUrl,name:e.name,response:Promise.resolve({ok:!0,arrayBuffer:()=>t,json:()=>JSON.parse(new TextDecoder("utf-8").decode(t)),text:()=>{throw new Error("NotImplementedException")},headers:{get:()=>{}}})},e.pendingDownloadInternal.response}const t=e.loadRemote&&Pe.config.remoteSources?Pe.config.remoteSources:[""];let o;for(let n of t){n=n.trim(),"./"===n&&(n="");const t=le(e,n);e.name===t?Pe.diagnosticTracing&&b(`Attempting to download '${t}'`):Pe.diagnosticTracing&&b(`Attempting to download '${t}' for ${e.name}`);try{e.resolvedUrl=t;const n=fe(e);if(e.pendingDownloadInternal=n,o=await n.response,!o||!o.ok)continue;return o}catch(e){o||(o={ok:!1,url:t,status:0,statusText:""+e});continue}}const n=e.isOptional||e.name.match(/\.pdb$/)&&Pe.config.ignorePdbLoadErrors;if(o||Be(!1,`Response undefined ${e.name}`),!n){const t=new Error(`download '${o.url}' for ${e.name} failed ${o.status} ${o.statusText}`);throw t.status=o.status,t}y(`optional download '${o.url}' for ${e.name} failed ${o.status} ${o.statusText}`)}(e);return t?(J[e.behavior]||(e.buffer=await t.arrayBuffer(),++Pe.actual_downloaded_assets_count),e):e}finally{if(--N,L&&N==Pe.maxParallelDownloads-1){Pe.diagnosticTracing&&b("Resuming more parallel downloads");const e=L;L=void 0,e.promise_control.resolve()}}}function le(e,t){let o;return null==t&&Be(!1,`sourcePrefix must be provided for ${e.name}`),e.resolvedUrl?o=e.resolvedUrl:(o=""===t?"assembly"===e.behavior||"pdb"===e.behavior?e.name:"resource"===e.behavior&&e.culture&&""!==e.culture?`${e.culture}/${e.name}`:e.name:t+e.name,o=ce(Pe.locateFile(o),e.behavior)),o&&"string"==typeof o||Be(!1,"attemptUrl need to be path or url string"),o}function ce(e,t){return Pe.modulesUniqueQuery&&q[t]&&(e+=Pe.modulesUniqueQuery),e}let de=0;const ue=new Set;function fe(e){try{e.resolvedUrl||Be(!1,"Request's resolvedUrl must be set");const t=function(e){let t=e.resolvedUrl;if(Pe.loadBootResource){const o=ge(e);if(o instanceof Promise)return o;"string"==typeof o&&(t=o)}const o={};return e.cache?o.cache=e.cache:Pe.config.disableNoCacheFetch||(o.cache="no-cache"),e.useCredentials?o.credentials="include":!Pe.config.disableIntegrityCheck&&e.hash&&(o.integrity=e.hash),Pe.fetch_like(t,o)}(e),o={name:e.name,url:e.resolvedUrl,response:t};return ue.add(e.name),o.response.then((()=>{"assembly"==e.behavior&&Pe.loadedAssemblies.push(e.name),de++,Pe.onDownloadResourceProgress&&Pe.onDownloadResourceProgress(de,ue.size)})),o}catch(t){const o={ok:!1,url:e.resolvedUrl,status:500,statusText:"ERR29: "+t,arrayBuffer:()=>{throw t},json:()=>{throw t}};return{name:e.name,url:e.resolvedUrl,response:Promise.resolve(o)}}}const me={resource:"assembly",assembly:"assembly",pdb:"pdb",icu:"globalization",vfs:"configuration",manifest:"manifest",dotnetwasm:"dotnetwasm","js-module-dotnet":"dotnetjs","js-module-native":"dotnetjs","js-module-runtime":"dotnetjs","js-module-threads":"dotnetjs"};function ge(e){var t;if(Pe.loadBootResource){const o=null!==(t=e.hash)&&void 0!==t?t:"",n=e.resolvedUrl,r=me[e.behavior];if(r){const t=Pe.loadBootResource(r,e.name,n,o,e.behavior);return"string"==typeof t?I(t):t}}}function pe(e){e.pendingDownloadInternal=null,e.pendingDownload=null,e.buffer=null,e.moduleExports=null}function he(e){let t=e.lastIndexOf("/");return t>=0&&t++,e.substring(t)}async function we(e){e&&await Promise.all((null!=e?e:[]).map((e=>async function(e){try{const t=e.name;if(!e.moduleExports){const o=ce(Pe.locateFile(t),"js-module-library-initializer");Pe.diagnosticTracing&&b(`Attempting to import '${o}' for ${e}`),e.moduleExports=await import(/*! webpackIgnore: true */o)}Pe.libraryInitializers.push({scriptName:t,exports:e.moduleExports})}catch(t){E(`Failed to import library initializer '${e}': ${t}`)}}(e))))}async function be(e,t){if(!Pe.libraryInitializers)return;const o=[];for(let n=0;n<Pe.libraryInitializers.length;n++){const r=Pe.libraryInitializers[n];r.exports[e]&&o.push(ye(r.scriptName,e,(()=>r.exports[e](...t))))}await Promise.all(o)}async function ye(e,t,o){try{await o()}catch(o){throw E(`Failed to invoke '${t}' on library initializer '${e}': ${o}`),Xe(1,o),o}}function ve(e,t){if(e===t)return e;const o={...t};return void 0!==o.assets&&o.assets!==e.assets&&(o.assets=[...e.assets||[],...o.assets||[]]),void 0!==o.resources&&(o.resources=_e(e.resources||{assembly:[],jsModuleNative:[],jsModuleRuntime:[],wasmNative:[]},o.resources)),void 0!==o.environmentVariables&&(o.environmentVariables={...e.environmentVariables||{},...o.environmentVariables||{}}),void 0!==o.runtimeOptions&&o.runtimeOptions!==e.runtimeOptions&&(o.runtimeOptions=[...e.runtimeOptions||[],...o.runtimeOptions||[]]),Object.assign(e,o)}function Ee(e,t){if(e===t)return e;const o={...t};return o.config&&(e.config||(e.config={}),o.config=ve(e.config,o.config)),Object.assign(e,o)}function _e(e,t){if(e===t)return e;const o={...t};return void 0!==o.coreAssembly&&(o.coreAssembly=[...e.coreAssembly||[],...o.coreAssembly||[]]),void 0!==o.assembly&&(o.assembly=[...e.assembly||[],...o.assembly||[]]),void 0!==o.lazyAssembly&&(o.lazyAssembly=[...e.lazyAssembly||[],...o.lazyAssembly||[]]),void 0!==o.corePdb&&(o.corePdb=[...e.corePdb||[],...o.corePdb||[]]),void 0!==o.pdb&&(o.pdb=[...e.pdb||[],...o.pdb||[]]),void 0!==o.jsModuleWorker&&(o.jsModuleWorker=[...e.jsModuleWorker||[],...o.jsModuleWorker||[]]),void 0!==o.jsModuleNative&&(o.jsModuleNative=[...e.jsModuleNative||[],...o.jsModuleNative||[]]),void 0!==o.jsModuleDiagnostics&&(o.jsModuleDiagnostics=[...e.jsModuleDiagnostics||[],...o.jsModuleDiagnostics||[]]),void 0!==o.jsModuleRuntime&&(o.jsModuleRuntime=[...e.jsModuleRuntime||[],...o.jsModuleRuntime||[]]),void 0!==o.wasmSymbols&&(o.wasmSymbols=[...e.wasmSymbols||[],...o.wasmSymbols||[]]),void 0!==o.wasmNative&&(o.wasmNative=[...e.wasmNative||[],...o.wasmNative||[]]),void 0!==o.icu&&(o.icu=[...e.icu||[],...o.icu||[]]),void 0!==o.satelliteResources&&(o.satelliteResources=function(e,t){if(e===t)return e;for(const o in t)e[o]=[...e[o]||[],...t[o]||[]];return e}(e.satelliteResources||{},o.satelliteResources||{})),void 0!==o.modulesAfterConfigLoaded&&(o.modulesAfterConfigLoaded=[...e.modulesAfterConfigLoaded||[],...o.modulesAfterConfigLoaded||[]]),void 0!==o.modulesAfterRuntimeReady&&(o.modulesAfterRuntimeReady=[...e.modulesAfterRuntimeReady||[],...o.modulesAfterRuntimeReady||[]]),void 0!==o.extensions&&(o.extensions={...e.extensions||{},...o.extensions||{}}),void 0!==o.vfs&&(o.vfs=[...e.vfs||[],...o.vfs||[]]),Object.assign(e,o)}function xe(){const e=Pe.config;if(e.environmentVariables=e.environmentVariables||{},e.runtimeOptions=e.runtimeOptions||[],e.resources=e.resources||{assembly:[],jsModuleNative:[],jsModuleWorker:[],jsModuleRuntime:[],wasmNative:[],vfs:[],satelliteResources:{}},e.assets){Pe.diagnosticTracing&&b("config.assets is deprecated, use config.resources instead");for(const t of e.assets){const o={};switch(t.behavior){case"assembly":o.assembly=[t];break;case"pdb":o.pdb=[t];break;case"resource":o.satelliteResources={},o.satelliteResources[t.culture]=[t];break;case"icu":o.icu=[t];break;case"symbols":o.wasmSymbols=[t];break;case"vfs":o.vfs=[t];break;case"dotnetwasm":o.wasmNative=[t];break;case"js-module-threads":o.jsModuleWorker=[t];break;case"js-module-runtime":o.jsModuleRuntime=[t];break;case"js-module-native":o.jsModuleNative=[t];break;case"js-module-diagnostics":o.jsModuleDiagnostics=[t];break;case"js-module-dotnet":break;default:throw new Error(`Unexpected behavior ${t.behavior} of asset ${t.name}`)}_e(e.resources,o)}}e.debugLevel,e.applicationEnvironment||(e.applicationEnvironment="Production"),e.applicationCulture&&(e.environmentVariables.LANG=`${e.applicationCulture}.UTF-8`),Ue.diagnosticTracing=Pe.diagnosticTracing=!!e.diagnosticTracing,Ue.waitForDebugger=e.waitForDebugger,Pe.maxParallelDownloads=e.maxParallelDownloads||Pe.maxParallelDownloads,Pe.enableDownloadRetry=void 0!==e.enableDownloadRetry?e.enableDownloadRetry:Pe.enableDownloadRetry}let je=!1;async function Re(e){var t;if(je)return void await Pe.afterConfigLoaded.promise;let o;try{if(e.configSrc||Pe.config&&0!==Object.keys(Pe.config).length&&(Pe.config.assets||Pe.config.resources)||(e.configSrc="dotnet.boot.js"),o=e.configSrc,je=!0,o&&(Pe.diagnosticTracing&&b("mono_wasm_load_config"),await async function(e){const t=e.configSrc,o=Pe.locateFile(t);let n=null;void 0!==Pe.loadBootResource&&(n=Pe.loadBootResource("manifest",t,o,"","manifest"));let r,i=null;if(n)if("string"==typeof n)n.includes(".json")?(i=await s(I(n)),r=await Ae(i)):r=(await import(I(n))).config;else{const e=await n;"function"==typeof e.json?(i=e,r=await Ae(i)):r=e.config}else o.includes(".json")?(i=await s(ce(o,"manifest")),r=await Ae(i)):r=(await import(ce(o,"manifest"))).config;function s(e){return Pe.fetch_like(e,{method:"GET",credentials:"include",cache:"no-cache"})}Pe.config.applicationEnvironment&&(r.applicationEnvironment=Pe.config.applicationEnvironment),ve(Pe.config,r)}(e)),xe(),await we(null===(t=Pe.config.resources)||void 0===t?void 0:t.modulesAfterConfigLoaded),await be("onRuntimeConfigLoaded",[Pe.config]),e.onConfigLoaded)try{await e.onConfigLoaded(Pe.config,Le),xe()}catch(e){throw _("onConfigLoaded() failed",e),e}xe(),Pe.afterConfigLoaded.promise_control.resolve(Pe.config)}catch(t){const n=`Failed to load config file ${o} ${t} ${null==t?void 0:t.stack}`;throw Pe.config=e.config=Object.assign(Pe.config,{message:n,error:t,isError:!0}),Xe(1,new Error(n)),t}}function Te(){return!!globalThis.navigator&&(Pe.isChromium||Pe.isFirefox)}async function Ae(e){const t=Pe.config,o=await e.json();t.applicationEnvironment||o.applicationEnvironment||(o.applicationEnvironment=e.headers.get("Blazor-Environment")||e.headers.get("DotNet-Environment")||void 0),o.environmentVariables||(o.environmentVariables={});const n=e.headers.get("DOTNET-MODIFIABLE-ASSEMBLIES");n&&(o.environmentVariables.DOTNET_MODIFIABLE_ASSEMBLIES=n);const r=e.headers.get("ASPNETCORE-BROWSER-TOOLS");return r&&(o.environmentVariables.__ASPNETCORE_BROWSER_TOOLS=r),o}"function"!=typeof importScripts||globalThis.onmessage||(globalThis.dotnetSidecar=!0);const Se="object"==typeof process&&"object"==typeof process.versions&&"string"==typeof process.versions.node,De="function"==typeof importScripts,Oe=De&&"undefined"!=typeof dotnetSidecar,Ce=De&&!Oe,ke="object"==typeof window||De&&!Se,Ie=!ke&&!Se;let Ue={},Pe={},Me={},Le={},Ne={},$e=!1;const ze={},We={config:ze},Fe={mono:{},binding:{},internal:Ne,module:We,loaderHelpers:Pe,runtimeHelpers:Ue,diagnosticHelpers:Me,api:Le};function Be(e,t){if(e)return;const o="Assert failed: "+("function"==typeof t?t():t),n=new Error(o);_(o,n),Ue.nativeAbort(n)}function Ve(){return void 0!==Pe.exitCode}function qe(){return Ue.runtimeReady&&!Ve()}function He(){Ve()&&Be(!1,`.NET runtime already exited with ${Pe.exitCode} ${Pe.exitReason}. You can use runtime.runMain() which doesn't exit the runtime.`),Ue.runtimeReady||Be(!1,".NET runtime didn't start yet. Please call dotnet.create() first.")}function Je(){ke&&(globalThis.addEventListener("unhandledrejection",et),globalThis.addEventListener("error",tt))}let Ze,Qe;function Ge(e){Qe&&Qe(e),Xe(e,Pe.exitReason)}function Ke(e){Ze&&Ze(e||Pe.exitReason),Xe(1,e||Pe.exitReason)}function Xe(t,o){var n,r;const i=o&&"object"==typeof o;t=i&&"number"==typeof o.status?o.status:void 0===t?-1:t;const s=i&&"string"==typeof o.message?o.message:""+o;(o=i?o:Ue.ExitStatus?function(e,t){const o=new Ue.ExitStatus(e);return o.message=t,o.toString=()=>t,o}(t,s):new Error("Exit with code "+t+" "+s)).status=t,o.message||(o.message=s);const a=""+(o.stack||(new Error).stack);try{Object.defineProperty(o,"stack",{get:()=>a})}catch(e){}const l=!!o.silent;if(o.silent=!0,Ve())Pe.diagnosticTracing&&b("mono_exit called after exit");else{try{We.onAbort==Ke&&(We.onAbort=Ze),We.onExit==Ge&&(We.onExit=Qe),ke&&(globalThis.removeEventListener("unhandledrejection",et),globalThis.removeEventListener("error",tt)),Ue.runtimeReady?(Ue.jiterpreter_dump_stats&&Ue.jiterpreter_dump_stats(!1),0===t&&(null===(n=Pe.config)||void 0===n?void 0:n.interopCleanupOnExit)&&Ue.forceDisposeProxies(!0,!0),e&&0!==t&&(null===(r=Pe.config)||void 0===r||r.dumpThreadsOnNonZeroExit)):(Pe.diagnosticTracing&&b(`abort_startup, reason: ${o}`),function(e){Pe.allDownloadsQueued.promise_control.reject(e),Pe.allDownloadsFinished.promise_control.reject(e),Pe.afterConfigLoaded.promise_control.reject(e),Pe.wasmCompilePromise.promise_control.reject(e),Pe.runtimeModuleLoaded.promise_control.reject(e),Ue.dotnetReady&&(Ue.dotnetReady.promise_control.reject(e),Ue.afterInstantiateWasm.promise_control.reject(e),Ue.beforePreInit.promise_control.reject(e),Ue.afterPreInit.promise_control.reject(e),Ue.afterPreRun.promise_control.reject(e),Ue.beforeOnRuntimeInitialized.promise_control.reject(e),Ue.afterOnRuntimeInitialized.promise_control.reject(e),Ue.afterPostRun.promise_control.reject(e))}(o))}catch(e){E("mono_exit A failed",e)}try{l||(function(e,t){if(0!==e&&t){const e=Ue.ExitStatus&&t instanceof Ue.ExitStatus?b:_;"string"==typeof t?e(t):(void 0===t.stack&&(t.stack=(new Error).stack+""),t.message?e(Ue.stringify_as_error_with_stack?Ue.stringify_as_error_with_stack(t.message+"\n"+t.stack):t.message+"\n"+t.stack):e(JSON.stringify(t)))}!Ce&&Pe.config&&(Pe.config.logExitCode?Pe.config.forwardConsoleLogsToWS?R("WASM EXIT "+e):v("WASM EXIT "+e):Pe.config.forwardConsoleLogsToWS&&R())}(t,o),function(e){if(ke&&!Ce&&Pe.config&&Pe.config.appendElementOnExit&&document){const t=document.createElement("label");t.id="tests_done",0!==e&&(t.style.background="red"),t.innerHTML=""+e,document.body.appendChild(t)}}(t))}catch(e){E("mono_exit B failed",e)}Pe.exitCode=t,Pe.exitReason||(Pe.exitReason=o),!Ce&&Ue.runtimeReady&&We.runtimeKeepalivePop()}if(Pe.config&&Pe.config.asyncFlushOnExit&&0===t)throw(async()=>{try{await async function(){try{const e=await import(/*! webpackIgnore: true */"process"),t=e=>new Promise(((t,o)=>{e.on("error",o),e.end("","utf8",t)})),o=t(e.stderr),n=t(e.stdout);let r;const i=new Promise((e=>{r=setTimeout((()=>e("timeout")),1e3)}));await Promise.race([Promise.all([n,o]),i]),clearTimeout(r)}catch(e){_(`flushing std* streams failed: ${e}`)}}()}finally{Ye(t,o)}})(),o;Ye(t,o)}function Ye(e,t){if(Ue.runtimeReady&&Ue.nativeExit)try{Ue.nativeExit(e)}catch(e){!Ue.ExitStatus||e instanceof Ue.ExitStatus||E("set_exit_code_and_quit_now failed: "+e.toString())}if(0!==e||!ke)throw Se&&Ne.process?Ne.process.exit(e):Ue.quit&&Ue.quit(e,t),t}function et(e){ot(e,e.reason,"rejection")}function tt(e){ot(e,e.error,"error")}function ot(e,t,o){e.preventDefault();try{t||(t=new Error("Unhandled "+o)),void 0===t.stack&&(t.stack=(new Error).stack),t.stack=t.stack+"",t.silent||(_("Unhandled error:",t),Xe(1,t))}catch(e){}}!function(e){if($e)throw new Error("Loader module already loaded");$e=!0,Ue=e.runtimeHelpers,Pe=e.loaderHelpers,Me=e.diagnosticHelpers,Le=e.api,Ne=e.internal,Object.assign(Le,{INTERNAL:Ne,invokeLibraryInitializers:be}),Object.assign(e.module,{config:ve(ze,{environmentVariables:{}})});const r={mono_wasm_bindings_is_ready:!1,config:e.module.config,diagnosticTracing:!1,nativeAbort:e=>{throw e||new Error("abort")},nativeExit:e=>{throw new Error("exit:"+e)}},l={gitHash:"95017c711e6afc1085133d440e42b4bd78155701",config:e.module.config,diagnosticTracing:!1,maxParallelDownloads:16,enableDownloadRetry:!0,_loaded_files:[],loadedFiles:[],loadedAssemblies:[],libraryInitializers:[],workerNextNumber:1,actual_downloaded_assets_count:0,actual_instantiated_assets_count:0,expected_downloaded_assets_count:0,expected_instantiated_assets_count:0,afterConfigLoaded:i(),allDownloadsQueued:i(),allDownloadsFinished:i(),wasmCompilePromise:i(),runtimeModuleLoaded:i(),loadingWorkers:i(),is_exited:Ve,is_runtime_running:qe,assert_runtime_running:He,mono_exit:Xe,createPromiseController:i,getPromiseController:s,assertIsControllablePromise:a,mono_download_assets:oe,resolve_single_asset_path:ee,setup_proxy_console:j,set_thread_prefix:w,installUnhandledErrorHandler:Je,retrieve_asset_download:ie,invokeLibraryInitializers:be,isDebuggingSupported:Te,exceptions:t,simd:n,relaxedSimd:o};Object.assign(Ue,r),Object.assign(Pe,l)}(Fe);let nt,rt,it,st=!1,at=!1;async function lt(e){if(!at){if(at=!0,ke&&Pe.config.forwardConsoleLogsToWS&&void 0!==globalThis.WebSocket&&j("main",globalThis.console,globalThis.location.origin),We||Be(!1,"Null moduleConfig"),Pe.config||Be(!1,"Null moduleConfig.config"),"function"==typeof e){const t=e(Fe.api);if(t.ready)throw new Error("Module.ready couldn't be redefined.");Object.assign(We,t),Ee(We,t)}else{if("object"!=typeof e)throw new Error("Can't use moduleFactory callback of createDotnetRuntime function.");Ee(We,e)}await async function(e){if(Se){const e=await import(/*! webpackIgnore: true */"process"),t=14;if(e.versions.node.split(".")[0]<t)throw new Error(`NodeJS at '${e.execPath}' has too low version '${e.versions.node}', please use at least ${t}. See also https://aka.ms/dotnet-wasm-features`)}const t=/*! webpackIgnore: true */import.meta.url,o=t.indexOf("?");var n;if(o>0&&(Pe.modulesUniqueQuery=t.substring(o)),Pe.scriptUrl=t.replace(/\\/g,"/").replace(/[?#].*/,""),Pe.scriptDirectory=(n=Pe.scriptUrl).slice(0,n.lastIndexOf("/"))+"/",Pe.locateFile=e=>"URL"in globalThis&&globalThis.URL!==C?new URL(e,Pe.scriptDirectory).toString():M(e)?e:Pe.scriptDirectory+e,Pe.fetch_like=k,Pe.out=console.log,Pe.err=console.error,Pe.onDownloadResourceProgress=e.onDownloadResourceProgress,ke&&globalThis.navigator){const e=globalThis.navigator,t=e.userAgentData&&e.userAgentData.brands;t&&t.length>0?Pe.isChromium=t.some((e=>"Google Chrome"===e.brand||"Microsoft Edge"===e.brand||"Chromium"===e.brand)):e.userAgent&&(Pe.isChromium=e.userAgent.includes("Chrome"),Pe.isFirefox=e.userAgent.includes("Firefox"))}Ne.require=Se?await import(/*! webpackIgnore: true */"module").then((e=>e.createRequire(/*! webpackIgnore: true */import.meta.url))):Promise.resolve((()=>{throw new Error("require not supported")})),void 0===globalThis.URL&&(globalThis.URL=C)}(We)}}async function ct(e){return await lt(e),Ze=We.onAbort,Qe=We.onExit,We.onAbort=Ke,We.onExit=Ge,We.ENVIRONMENT_IS_PTHREAD?async function(){(function(){const e=new MessageChannel,t=e.port1,o=e.port2;t.addEventListener("message",(e=>{var n,r;n=JSON.parse(e.data.config),r=JSON.parse(e.data.monoThreadInfo),st?Pe.diagnosticTracing&&b("mono config already received"):(ve(Pe.config,n),Ue.monoThreadInfo=r,xe(),Pe.diagnosticTracing&&b("mono config received"),st=!0,Pe.afterConfigLoaded.promise_control.resolve(Pe.config),ke&&n.forwardConsoleLogsToWS&&void 0!==globalThis.WebSocket&&Pe.setup_proxy_console("worker-idle",console,globalThis.location.origin)),t.close(),o.close()}),{once:!0}),t.start(),self.postMessage({[l]:{monoCmd:"preload",port:o}},[o])})(),await Pe.afterConfigLoaded.promise,function(){const e=Pe.config;e.assets||Be(!1,"config.assets must be defined");for(const t of e.assets)X(t),Q[t.behavior]&&z.push(t)}(),setTimeout((async()=>{try{await oe()}catch(e){Xe(1,e)}}),0);const e=dt(),t=await Promise.all(e);return await ut(t),We}():async function(){var e;await Re(We),re();const t=dt();(async function(){try{const e=ee("dotnetwasm");await se(e),e&&e.pendingDownloadInternal&&e.pendingDownloadInternal.response||Be(!1,"Can't load dotnet.native.wasm");const t=await e.pendingDownloadInternal.response,o=t.headers&&t.headers.get?t.headers.get("Content-Type"):void 0;let n;if("function"==typeof WebAssembly.compileStreaming&&"application/wasm"===o)n=await WebAssembly.compileStreaming(t);else{ke&&"application/wasm"!==o&&E('WebAssembly resource does not have the expected content type "application/wasm", so falling back to slower ArrayBuffer instantiation.');const e=await t.arrayBuffer();Pe.diagnosticTracing&&b("instantiate_wasm_module buffered"),n=Ie?await Promise.resolve(new WebAssembly.Module(e)):await WebAssembly.compile(e)}e.pendingDownloadInternal=null,e.pendingDownload=null,e.buffer=null,e.moduleExports=null,Pe.wasmCompilePromise.promise_control.resolve(n)}catch(e){Pe.wasmCompilePromise.promise_control.reject(e)}})(),setTimeout((async()=>{try{D(),await oe()}catch(e){Xe(1,e)}}),0);const o=await Promise.all(t);return await ut(o),await Ue.dotnetReady.promise,await we(null===(e=Pe.config.resources)||void 0===e?void 0:e.modulesAfterRuntimeReady),await be("onRuntimeReady",[Fe.api]),Le}()}function dt(){const e=ee("js-module-runtime"),t=ee("js-module-native");if(nt&&rt)return[nt,rt,it];"object"==typeof e.moduleExports?nt=e.moduleExports:(Pe.diagnosticTracing&&b(`Attempting to import '${e.resolvedUrl}' for ${e.name}`),nt=import(/*! webpackIgnore: true */e.resolvedUrl)),"object"==typeof t.moduleExports?rt=t.moduleExports:(Pe.diagnosticTracing&&b(`Attempting to import '${t.resolvedUrl}' for ${t.name}`),rt=import(/*! webpackIgnore: true */t.resolvedUrl));const o=Y("js-module-diagnostics");return o&&("object"==typeof o.moduleExports?it=o.moduleExports:(Pe.diagnosticTracing&&b(`Attempting to import '${o.resolvedUrl}' for ${o.name}`),it=import(/*! webpackIgnore: true */o.resolvedUrl))),[nt,rt,it]}async function ut(e){const{initializeExports:t,initializeReplacements:o,configureRuntimeStartup:n,configureEmscriptenStartup:r,configureWorkerStartup:i,setRuntimeGlobals:s,passEmscriptenInternals:a}=e[0],{default:l}=e[1],c=e[2];s(Fe),t(Fe),c&&c.setRuntimeGlobals(Fe),await n(We),Pe.runtimeModuleLoaded.promise_control.resolve(),l((e=>(Object.assign(We,{ready:e.ready,__dotnet_runtime:{initializeReplacements:o,configureEmscriptenStartup:r,configureWorkerStartup:i,passEmscriptenInternals:a}}),We))).catch((e=>{if(e.message&&e.message.toLowerCase().includes("out of memory"))throw new Error(".NET runtime has failed to start, because too much memory was requested. Please decrease the memory by adjusting EmccMaximumHeapSize. See also https://aka.ms/dotnet-wasm-features");throw e}))}const ft=new class{withModuleConfig(e){try{return Ee(We,e),this}catch(e){throw Xe(1,e),e}}withOnConfigLoaded(e){try{return Ee(We,{onConfigLoaded:e}),this}catch(e){throw Xe(1,e),e}}withConsoleForwarding(){try{return ve(ze,{forwardConsoleLogsToWS:!0}),this}catch(e){throw Xe(1,e),e}}withExitOnUnhandledError(){try{return ve(ze,{exitOnUnhandledError:!0}),Je(),this}catch(e){throw Xe(1,e),e}}withAsyncFlushOnExit(){try{return ve(ze,{asyncFlushOnExit:!0}),this}catch(e){throw Xe(1,e),e}}withExitCodeLogging(){try{return ve(ze,{logExitCode:!0}),this}catch(e){throw Xe(1,e),e}}withElementOnExit(){try{return ve(ze,{appendElementOnExit:!0}),this}catch(e){throw Xe(1,e),e}}withInteropCleanupOnExit(){try{return ve(ze,{interopCleanupOnExit:!0}),this}catch(e){throw Xe(1,e),e}}withDumpThreadsOnNonZeroExit(){try{return ve(ze,{dumpThreadsOnNonZeroExit:!0}),this}catch(e){throw Xe(1,e),e}}withWaitingForDebugger(e){try{return ve(ze,{waitForDebugger:e}),this}catch(e){throw Xe(1,e),e}}withInterpreterPgo(e,t){try{return ve(ze,{interpreterPgo:e,interpreterPgoSaveDelay:t}),ze.runtimeOptions?ze.runtimeOptions.push("--interp-pgo-recording"):ze.runtimeOptions=["--interp-pgo-recording"],this}catch(e){throw Xe(1,e),e}}withConfig(e){try{return ve(ze,e),this}catch(e){throw Xe(1,e),e}}withConfigSrc(e){try{return e&&"string"==typeof e||Be(!1,"must be file path or URL"),Ee(We,{configSrc:e}),this}catch(e){throw Xe(1,e),e}}withVirtualWorkingDirectory(e){try{return e&&"string"==typeof e||Be(!1,"must be directory path"),ve(ze,{virtualWorkingDirectory:e}),this}catch(e){throw Xe(1,e),e}}withEnvironmentVariable(e,t){try{const o={};return o[e]=t,ve(ze,{environmentVariables:o}),this}catch(e){throw Xe(1,e),e}}withEnvironmentVariables(e){try{return e&&"object"==typeof e||Be(!1,"must be dictionary object"),ve(ze,{environmentVariables:e}),this}catch(e){throw Xe(1,e),e}}withDiagnosticTracing(e){try{return"boolean"!=typeof e&&Be(!1,"must be boolean"),ve(ze,{diagnosticTracing:e}),this}catch(e){throw Xe(1,e),e}}withDebugging(e){try{return null!=e&&"number"==typeof e||Be(!1,"must be number"),ve(ze,{debugLevel:e}),this}catch(e){throw Xe(1,e),e}}withApplicationArguments(...e){try{return e&&Array.isArray(e)||Be(!1,"must be array of strings"),ve(ze,{applicationArguments:e}),this}catch(e){throw Xe(1,e),e}}withRuntimeOptions(e){try{return e&&Array.isArray(e)||Be(!1,"must be array of strings"),ze.runtimeOptions?ze.runtimeOptions.push(...e):ze.runtimeOptions=e,this}catch(e){throw Xe(1,e),e}}withMainAssembly(e){try{return ve(ze,{mainAssemblyName:e}),this}catch(e){throw Xe(1,e),e}}withApplicationArgumentsFromQuery(){try{if(!globalThis.window)throw new Error("Missing window to the query parameters from");if(void 0===globalThis.URLSearchParams)throw new Error("URLSearchParams is supported");const e=new URLSearchParams(globalThis.window.location.search).getAll("arg");return this.withApplicationArguments(...e)}catch(e){throw Xe(1,e),e}}withApplicationEnvironment(e){try{return ve(ze,{applicationEnvironment:e}),this}catch(e){throw Xe(1,e),e}}withApplicationCulture(e){try{return ve(ze,{applicationCulture:e}),this}catch(e){throw Xe(1,e),e}}withResourceLoader(e){try{return Pe.loadBootResource=e,this}catch(e){throw Xe(1,e),e}}async download(){try{await async function(){lt(We),await Re(We),re(),D(),oe(),await Pe.allDownloadsFinished.promise}()}catch(e){throw Xe(1,e),e}}async create(){try{return this.instance||(this.instance=await async function(){return await ct(We),Fe.api}()),this.instance}catch(e){throw Xe(1,e),e}}async run(){try{return We.config||Be(!1,"Null moduleConfig.config"),this.instance||await this.create(),this.instance.runMainAndExit()}catch(e){throw Xe(1,e),e}}},mt=Xe,gt=ct;Ie||"function"==typeof globalThis.URL||Be(!1,"This browser/engine doesn't support URL API. Please use a modern version. See also https://aka.ms/dotnet-wasm-features"),"function"!=typeof globalThis.BigInt64Array&&Be(!1,"This browser/engine doesn't support BigInt64Array API. Please use a modern version. See also https://aka.ms/dotnet-wasm-features"),ft.withConfig(/*json-start*/{
  "mainAssemblyName": "BlazorWasmPortfolioGhAction",
  "resources": {
    "hash": "sha256-SLJZchqoVmXrDoj+rqkwZgU5g03WSYYNkwMok3gpGEA=",
    "jsModuleNative": [
      {
        "name": "dotnet.native.nv0vmv8vjv.js"
      }
    ],
    "jsModuleRuntime": [
      {
        "name": "dotnet.runtime.v06hirbjsv.js"
      }
    ],
    "wasmNative": [
      {
        "name": "dotnet.native.9g0wv54x1o.wasm",
        "hash": "sha256-Sf1J6tZ0m2dCrfg5LrsYFqiUNoSBw9Nk5MDHON6eUrc=",
        "cache": "force-cache"
      }
    ],
    "icu": [
      {
        "virtualPath": "icudt.dat",
        "name": "icudt.oh1zvcfom8.dat",
        "hash": "sha256-tO5O5YzMTVSaKBboxAqezOQL9ewmupzV2JrB5Rkc8a4=",
        "cache": "force-cache"
      }
    ],
    "coreAssembly": [
      {
        "virtualPath": "System.Runtime.InteropServices.JavaScript.wasm",
        "name": "System.Runtime.InteropServices.JavaScript.nfpi6j529e.wasm",
        "hash": "sha256-vewFTGYJ6sqGHLRhNEiGUx/yYornqXSIciph1rcmjrk=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Private.CoreLib.wasm",
        "name": "System.Private.CoreLib.kx3elhj8r7.wasm",
        "hash": "sha256-aFRQMNzUkANAjKGoANqR+LB+1LdZ5yhI5I7iBBTX2Rw=",
        "cache": "force-cache"
      }
    ],
    "assembly": [
      {
        "virtualPath": "AutoMapper.wasm",
        "name": "AutoMapper.totk3ghnq9.wasm",
        "hash": "sha256-szmkSB5fPdjK2TpIYWEG1ESlm4/hlclUj8URR7+K+xI=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Blazor.Extensions.Canvas.JS.wasm",
        "name": "Blazor.Extensions.Canvas.JS.iokb7a5y98.wasm",
        "hash": "sha256-ctyeKHsvFswpiKQJpTUUWdSrDGr4UNirukOrWYnwW78=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Blazor.Extensions.Canvas.wasm",
        "name": "Blazor.Extensions.Canvas.45bjdsigtk.wasm",
        "hash": "sha256-2AOrAB5tgoiK9wMMmwiE/pKPAtTjzgFudxSl6g4dt9s=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "BlazorComponentBus.wasm",
        "name": "BlazorComponentBus.z4ndupmmxd.wasm",
        "hash": "sha256-Y3iusDEqPeNKFX+L5jKWnvlAsAk2Y+TOoplZQZ1q7IE=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Blazored.LocalStorage.wasm",
        "name": "Blazored.LocalStorage.12n6dz54qr.wasm",
        "hash": "sha256-OaMAAd5n7ORfyur5e3QIyEVKJ76MKIvwbg7/icnnYcU=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "GoogleMapsComponents.wasm",
        "name": "GoogleMapsComponents.efztafmkcp.wasm",
        "hash": "sha256-ZmHCbg/jzmu3L1yRGrGoY5Ku0xYHqV1E3or0/+o7YOc=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Bogus.wasm",
        "name": "Bogus.zoyvwhk0j4.wasm",
        "hash": "sha256-YwVwZX6tWOwf8aZv2Qsc7rdM4dHWMvaodaUbJQDKdME=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "ManuHub.Blazor.Wasm.BrowserStorage.wasm",
        "name": "ManuHub.Blazor.Wasm.BrowserStorage.zklp6qk1k8.wasm",
        "hash": "sha256-fdivK8bPSEWsZuYnEzJoOobgkTwJqnqrGfqj2W4TfS4=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Fluxor.wasm",
        "name": "Fluxor.9yjucuf12h.wasm",
        "hash": "sha256-f0uhXaNzSHCguW7IErTRjeiGQXPWEWl5p/eehMZkDdc=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Fluxor.Blazor.Web.wasm",
        "name": "Fluxor.Blazor.Web.4opwue5a6j.wasm",
        "hash": "sha256-DBC32gXCmb31ELcLWyN0VaYPespDZiNUhIL276D1X7Q=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Fluxor.Blazor.Web.ReduxDevTools.wasm",
        "name": "Fluxor.Blazor.Web.ReduxDevTools.uoth4ri6xt.wasm",
        "hash": "sha256-ct9jjJSet5i5XkmhGNvZVQG35IGg3+kKEYLHP5CWaPc=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "GraphQL.Client.wasm",
        "name": "GraphQL.Client.5auog8a1ms.wasm",
        "hash": "sha256-s0CEW3wc62uD93Qb6rqCIRKQKclwkdjhdUaqysib3Ts=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "GraphQL.Client.Abstractions.wasm",
        "name": "GraphQL.Client.Abstractions.detub1lyzm.wasm",
        "hash": "sha256-VU9GfG0Ja4ILKn/SRdoxV+4a4qNhRS8WN5CFCFCm0NI=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "GraphQL.Client.Abstractions.Websocket.wasm",
        "name": "GraphQL.Client.Abstractions.Websocket.uvg77n0789.wasm",
        "hash": "sha256-xstCDdhG3LqzQ37sW7aw7XplbqJLh9zwxQr+1KTW8Pw=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "GraphQL.Client.Serializer.SystemTextJson.wasm",
        "name": "GraphQL.Client.Serializer.SystemTextJson.93ohp8tf7x.wasm",
        "hash": "sha256-Nf3XVZnaVcUHxI19a/gG+a3gxN/3cR6G3OrGDAp/BmM=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "GraphQL.Primitives.wasm",
        "name": "GraphQL.Primitives.tyvml3hbdg.wasm",
        "hash": "sha256-VTy0795HBio5vklt9z+WCljbNtrFuPZviHk6eyeou6I=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "HtmlAgilityPack.wasm",
        "name": "HtmlAgilityPack.yc53q646r7.wasm",
        "hash": "sha256-HtCqzZx+jaWhlVaRtXZuEjGE0pszwNdlnBubNEzeBCY=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Humanizer.wasm",
        "name": "Humanizer.oqup3v7t3k.wasm",
        "hash": "sha256-4NbSboZzzP9nikRtXapUZNzOyITt7ht9TNqCIQHr5OE=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Component.wasm",
        "name": "Component.z46u47etdp.wasm",
        "hash": "sha256-k5q3OHwkXEsgvjxzbxObQVTBcRdtADRc6flS380UmAE=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Markdig.wasm",
        "name": "Markdig.82zk74d1io.wasm",
        "hash": "sha256-gPUwGTHTe/sd4xmteeOSYd4a/J4deDy13pbwodSYfwc=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "MathNet.Numerics.wasm",
        "name": "MathNet.Numerics.4iylnyy3ah.wasm",
        "hash": "sha256-7MB/AdpJ1M5QLPfFuGFjCLGIJyd1QJSLoYMVWiC8/QU=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Meziantou.Framework.ByteSize.wasm",
        "name": "Meziantou.Framework.ByteSize.hzlxdhfsw3.wasm",
        "hash": "sha256-je62UDG2n4FgtBQYePHRQFFFCYxVEj87YI3rG7hgI7w=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Meziantou.Framework.CommandLine.wasm",
        "name": "Meziantou.Framework.CommandLine.cf6jnd1pbh.wasm",
        "hash": "sha256-uD2YUzM5cuux83RCM6DDdCNuzY6ik3uJC1DU8klSfOc=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Meziantou.Framework.JsonPath.wasm",
        "name": "Meziantou.Framework.JsonPath.zm1mk8m0au.wasm",
        "hash": "sha256-y8WCOev2SnUDjq1dYWuyQtLMpqmdS6rIR8VzOycWyeU=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Meziantou.Framework.Scheduling.wasm",
        "name": "Meziantou.Framework.Scheduling.t9kz5x7ire.wasm",
        "hash": "sha256-n3Wacg6nDySreGXBg4K2ztPYzQRrLMj+EN3Z/JYtbjw=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Meziantou.Framework.TextDiff.wasm",
        "name": "Meziantou.Framework.TextDiff.b5lgch2ioi.wasm",
        "hash": "sha256-8SD9Or/BFhOs/VZ9Xk5GJmdGZvjt/AcBu7bflVJuKVA=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Meziantou.Framework.Unicode.wasm",
        "name": "Meziantou.Framework.Unicode.8uiujibknb.wasm",
        "hash": "sha256-IBpuSko7SCvowQHHmcMjVjfaml+khFEA47033oYS+PQ=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Meziantou.GitLabClient.wasm",
        "name": "Meziantou.GitLabClient.yeouxy265g.wasm",
        "hash": "sha256-5v9pH9km7+fTqyAl0PqIrFrTL7EU3SZfffdtoU1hKac=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.AspNetCore.Authorization.wasm",
        "name": "Microsoft.AspNetCore.Authorization.c34a6yxwq6.wasm",
        "hash": "sha256-ONHV19xBFJR4lzxqLBusiYZFlZpz+79C2VtX9Y6XBnE=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.AspNetCore.Components.wasm",
        "name": "Microsoft.AspNetCore.Components.pjjudxdh1k.wasm",
        "hash": "sha256-dR8ZCoo7bB0MNDahdxPrsQo8rBqKBozwiaoOsNweXpQ=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.AspNetCore.Components.Authorization.wasm",
        "name": "Microsoft.AspNetCore.Components.Authorization.yif3dk0ero.wasm",
        "hash": "sha256-Wu62j2GnsEUn9q3qO0Q3/4PjkisI91WzT9c4yCkEg60=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.AspNetCore.Components.Forms.wasm",
        "name": "Microsoft.AspNetCore.Components.Forms.b24woh2l8m.wasm",
        "hash": "sha256-bcfDIbBb+BrCUOg0pl0YQ+RdHAsCBDp7m53WfYlj4is=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.AspNetCore.Components.Web.wasm",
        "name": "Microsoft.AspNetCore.Components.Web.w3qgxpx72t.wasm",
        "hash": "sha256-1ILxDNH7lOXTSlSblOIL4rKjbdzmA7IYPlMFaTk6cTo=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.AspNetCore.Components.WebAssembly.wasm",
        "name": "Microsoft.AspNetCore.Components.WebAssembly.98v8bok77a.wasm",
        "hash": "sha256-52QA9a9YdZTrEfFJfybxQQlDcBg8hQRzKR89oaZavp0=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.AspNetCore.Metadata.wasm",
        "name": "Microsoft.AspNetCore.Metadata.y7dnl79y3a.wasm",
        "hash": "sha256-UpzzxRBt/A3SImOi7qlz4YjRNENhFAQugJLtA6ksCjA=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.AspNetCore.WebUtilities.wasm",
        "name": "Microsoft.AspNetCore.WebUtilities.j7mawjhcv3.wasm",
        "hash": "sha256-ERUktVwftxbsjPbWCzScACp7xorFMJGwvuv07Dg8jO0=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.Extensions.Configuration.wasm",
        "name": "Microsoft.Extensions.Configuration.hg26ls8hcl.wasm",
        "hash": "sha256-D8ncaoAnTAqGlx+gCUyIlfqrTlrXRWcv+5mmJQFNu3o=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.Extensions.Configuration.Abstractions.wasm",
        "name": "Microsoft.Extensions.Configuration.Abstractions.ybbtbfajv9.wasm",
        "hash": "sha256-YjrpIR44XAWXdvhZSwdt+Oc0+Ro1uPrwJRVh0TooATQ=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.Extensions.Configuration.Binder.wasm",
        "name": "Microsoft.Extensions.Configuration.Binder.8vh8tf7y6x.wasm",
        "hash": "sha256-dNX0jS/PyQJkoKJcK4c+ZLvFeHw8Qc/Rt+H/6uDS1yk=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.Extensions.Configuration.Json.wasm",
        "name": "Microsoft.Extensions.Configuration.Json.vejc7kiu4g.wasm",
        "hash": "sha256-R8Rvf+ufvlMX05PSWGUXwGm3hty6uCI9EtqBH2FpDIQ=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.Extensions.DependencyInjection.wasm",
        "name": "Microsoft.Extensions.DependencyInjection.39rq5qhzej.wasm",
        "hash": "sha256-d6lEFD/IVLtWxCVMIxcNqclomc5lDC6mo7CTFAjWNnM=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.Extensions.DependencyInjection.Abstractions.wasm",
        "name": "Microsoft.Extensions.DependencyInjection.Abstractions.y1uw43b6zz.wasm",
        "hash": "sha256-2v/nZl0x9j2wzjtTh7TLqLm7pAWCAwMAMyRt9nZpVKk=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.Extensions.Diagnostics.wasm",
        "name": "Microsoft.Extensions.Diagnostics.9k7gklzza3.wasm",
        "hash": "sha256-SYLTEt2DTCtUluoNTbHyenQ206zhP8tHa/eYkEE48Vo=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.Extensions.Diagnostics.Abstractions.wasm",
        "name": "Microsoft.Extensions.Diagnostics.Abstractions.xgtxfsk0cx.wasm",
        "hash": "sha256-cbLP+c3z02AoaYnHKKVv0zoEw2as5tY8p4GuPVlBilw=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.Extensions.Http.wasm",
        "name": "Microsoft.Extensions.Http.trqjvnjg44.wasm",
        "hash": "sha256-t4ZMUpWWomu3AEu1/lJa2jFarEljSe5y6Gs4pjzL9Vg=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.Extensions.Localization.wasm",
        "name": "Microsoft.Extensions.Localization.mml9tf1r1w.wasm",
        "hash": "sha256-QoEBMwS9x0z4GTxqF5jsGvbmNw2WPeQ96y2xiLWOn1o=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.Extensions.Localization.Abstractions.wasm",
        "name": "Microsoft.Extensions.Localization.Abstractions.g0xf2olbpe.wasm",
        "hash": "sha256-IPnXRXBzt1mL/8UhbHABXJz1vDm9fqQiNSrR1Pwxkhs=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.Extensions.Logging.wasm",
        "name": "Microsoft.Extensions.Logging.an4mzjd62p.wasm",
        "hash": "sha256-BkB6cp9ZEnAuv+R9Osvum9gVeD7WwtjdzavxGMLzQqY=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.Extensions.Logging.Abstractions.wasm",
        "name": "Microsoft.Extensions.Logging.Abstractions.oo9o42rfnx.wasm",
        "hash": "sha256-CIOkfq3wr8EjrYzBw3siXJOvN4clHSPdHtxADPUVoFQ=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.Extensions.Options.wasm",
        "name": "Microsoft.Extensions.Options.jt1f41sp5n.wasm",
        "hash": "sha256-CSGpqQ6AW+vZqlhxb1cQoqGsLeDkgvg+Ghbnf/gzWJI=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.Extensions.Primitives.wasm",
        "name": "Microsoft.Extensions.Primitives.atliz8yjha.wasm",
        "hash": "sha256-7kFLmdt/aFQHRvIcCXm08haCdapsnhFuvGNj0cEaJ3Y=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.Extensions.Validation.wasm",
        "name": "Microsoft.Extensions.Validation.1igmz3eei4.wasm",
        "hash": "sha256-CUwN0UEngVN3nal+jQtEX6uTejg/tFi0raGAfpTxz0c=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.IdentityModel.Abstractions.wasm",
        "name": "Microsoft.IdentityModel.Abstractions.xa7xcdcqez.wasm",
        "hash": "sha256-OcMGxeMzQc19yoWpWnMOmGgfo21Noemddi2oyv3EvTg=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.IdentityModel.JsonWebTokens.wasm",
        "name": "Microsoft.IdentityModel.JsonWebTokens.b397lqmarc.wasm",
        "hash": "sha256-x+Yow19DogdXFqmBnYUWMcOIqhpGUL8YV9+GG1cUOB4=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.IdentityModel.Logging.wasm",
        "name": "Microsoft.IdentityModel.Logging.bx9o6sxbp1.wasm",
        "hash": "sha256-P85vgfGY5Bd5cQgY9kzCFC7ttblLqAdcLivwfsTN4Hg=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.IdentityModel.Tokens.wasm",
        "name": "Microsoft.IdentityModel.Tokens.ol6jhnl000.wasm",
        "hash": "sha256-1JrdKKrE+kDLZUMm3pFCHBjxlomKxAUkwMiJODQrQgU=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.IO.RecyclableMemoryStream.wasm",
        "name": "Microsoft.IO.RecyclableMemoryStream.k69j9tcsp2.wasm",
        "hash": "sha256-PLLNYyORp9p97V0x5KgrbBal9u/7enJGj68o7bI3pgU=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.JSInterop.wasm",
        "name": "Microsoft.JSInterop.0ijs9qr7lr.wasm",
        "hash": "sha256-uBLLVSI3odIEWZtSbowiX2FiQcjmOGtY7T/7noPVfIw=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.JSInterop.WebAssembly.wasm",
        "name": "Microsoft.JSInterop.WebAssembly.nsfh695mwg.wasm",
        "hash": "sha256-N8RKL2Eil4ZFxIkWa6Uig12+nRHOstda562q7GCs9fE=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Newtonsoft.Json.wasm",
        "name": "Newtonsoft.Json.jcjjiqe038.wasm",
        "hash": "sha256-s8KVuknfxWl1cuDvQM/OnpBfnpM1rxzvzq21S1cF36U=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "OneOf.wasm",
        "name": "OneOf.yq61etxwqn.wasm",
        "hash": "sha256-akqbZuRazMph7GZNcPHLIOp1mgnkjdaWZMM9Up4LPfE=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "QRCoder.wasm",
        "name": "QRCoder.jqr4n2c9hc.wasm",
        "hash": "sha256-ayRMl7l1GF4h65kNE0bKJ4lrGPZxmR+OkvPUFrQ11oU=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "ReverseMarkdown.wasm",
        "name": "ReverseMarkdown.kc67ln764z.wasm",
        "hash": "sha256-5PwSSx+yT7dQxnlMhB22BfjPF3Cb0fcErhP7+FKzLko=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Soenneker.Asyncs.Initializers.wasm",
        "name": "Soenneker.Asyncs.Initializers.5z5y75fk3r.wasm",
        "hash": "sha256-PyJx+paMQsWll6Qz5Vv0SKW3XHArIzszWrt+qwm5jjg=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Soenneker.Asyncs.Locks.wasm",
        "name": "Soenneker.Asyncs.Locks.eca2f31rpy.wasm",
        "hash": "sha256-IjR9krp46gLJ6La3sv7yPviIDMNKpOHX3kmuhl3q+Yc=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Soenneker.Atomics.Resources.wasm",
        "name": "Soenneker.Atomics.Resources.4accj3u8s0.wasm",
        "hash": "sha256-LIjRSnYyhyBAxbrD1eXfF7zTDvzt7c2Kudm0IhJXa2M=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Soenneker.Atomics.ValueBools.wasm",
        "name": "Soenneker.Atomics.ValueBools.gzr8oa4mi6.wasm",
        "hash": "sha256-VIA4rusP1snhCmzG4obSkyKAEticu5kvEf7xUKhG1Bk=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Soenneker.Atomics.ValueInts.wasm",
        "name": "Soenneker.Atomics.ValueInts.e1pf3ylegj.wasm",
        "hash": "sha256-fljuOIgXYIeLrFixpeG+j8MSyZGQEWHqwgZiAoe+R7Y=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Soenneker.Atomics.ValueNullableBools.wasm",
        "name": "Soenneker.Atomics.ValueNullableBools.kite1pwfud.wasm",
        "hash": "sha256-FA7ImJPQ9W2G5y9CL7eceA8R3SGTXa7UxLZwFwIIYEo=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Soenneker.Blazor.CreditCards.wasm",
        "name": "Soenneker.Blazor.CreditCards.19ylvl4vl9.wasm",
        "hash": "sha256-W2wHZV72LIHmuE3VD5BuIZHqTXHvHdcg9IMC8BvkMJw=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Soenneker.Blazor.Extensions.EventCallback.wasm",
        "name": "Soenneker.Blazor.Extensions.EventCallback.7gghnbxktx.wasm",
        "hash": "sha256-hwAeVIPwZEITmnpNpGhRPoDFMGIJ84FIW4tTy8xeV5s=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Soenneker.Blazor.Turnstile.wasm",
        "name": "Soenneker.Blazor.Turnstile.bx4u6tpyr1.wasm",
        "hash": "sha256-kSEQt2GrjJvQ4yXD8CHOuOf82auE3bEj5L3cGrE/Olc=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Soenneker.Blazor.Utils.Ids.wasm",
        "name": "Soenneker.Blazor.Utils.Ids.5mj3iq9n71.wasm",
        "hash": "sha256-DdBOhFGjuc9a97By2kTvanyZ6NjKuymjsSYdeijOJzk=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Soenneker.Blazor.Utils.JsVariable.wasm",
        "name": "Soenneker.Blazor.Utils.JsVariable.1h32wkxtf5.wasm",
        "hash": "sha256-60HEPtZXDfHA2HCFVHTumaxuIElAJwB4G4ee5lEoZUw=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Soenneker.Blazor.Utils.ModuleImport.wasm",
        "name": "Soenneker.Blazor.Utils.ModuleImport.0o5kmw3mjy.wasm",
        "hash": "sha256-UKCHNcescmK1bDsYVQ6AvqfETh2kI45JYETaUN+HA40=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Soenneker.Blazor.Utils.ResourceLoader.wasm",
        "name": "Soenneker.Blazor.Utils.ResourceLoader.4lvuy382v7.wasm",
        "hash": "sha256-mLZirO19f0DE6UH3HbV9DZ9OxKQ/y2fhvwsbaFhoNIc=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Soenneker.Culture.English.US.wasm",
        "name": "Soenneker.Culture.English.US.hacbja0tgv.wasm",
        "hash": "sha256-B1Itpxpo2kWdtrfoDl1xx3iSYLbtiXChOA9FMiXJ5Tw=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Soenneker.Dictionaries.SingletonKeys.wasm",
        "name": "Soenneker.Dictionaries.SingletonKeys.nzqzko5kdr.wasm",
        "hash": "sha256-Y7fqVokTuhEBqdELPMyFwKcKHjDIul+6dAD2RXKq2Nw=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Soenneker.Dictionaries.Singletons.wasm",
        "name": "Soenneker.Dictionaries.Singletons.b02j5lr9jh.wasm",
        "hash": "sha256-FeYkjCjyvferFpFYo6YK/zcfmF14+1W0E45rV5X2xJM=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Soenneker.Enums.ContentKinds.wasm",
        "name": "Soenneker.Enums.ContentKinds.7whhnzkg9x.wasm",
        "hash": "sha256-15c8tYXUMqQmuEUqIGLPLLYjT5KuxohqRfAexcK7Fs8=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Soenneker.Enums.InitializationModes.wasm",
        "name": "Soenneker.Enums.InitializationModes.zmysqaeix6.wasm",
        "hash": "sha256-W8AVJykw71NkoQqe3EqehZVJVMRqnmBhqwGfbSwtBC4=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Soenneker.Enums.JsonLibrary.wasm",
        "name": "Soenneker.Enums.JsonLibrary.ue3sy09mal.wasm",
        "hash": "sha256-RnIqmGyeQOVloVs2qKSrAToFbiurVenByWl/Jf4cl5s=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Soenneker.Enums.JsonOptions.wasm",
        "name": "Soenneker.Enums.JsonOptions.jwfwt9lga1.wasm",
        "hash": "sha256-g2ns2B8BrHQ6qru0uxqHXgG2NwaXD3nNoNZP7ltR4xk=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Soenneker.Extensions.CancellationTokens.wasm",
        "name": "Soenneker.Extensions.CancellationTokens.kh4ezau5ny.wasm",
        "hash": "sha256-yGaQFDl04ESOpE5fIJBStQXOOwpw3tjhXM3v3xMKJDQ=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Soenneker.Extensions.Char.wasm",
        "name": "Soenneker.Extensions.Char.e767vhmv60.wasm",
        "hash": "sha256-1mEQf1lH1XaPBdzOWkkBBMcqRH1fxKRywNTtKDbOqjg=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Soenneker.Extensions.Enumerable.wasm",
        "name": "Soenneker.Extensions.Enumerable.spy22c5yyk.wasm",
        "hash": "sha256-QN62jmbAWy4uvig+1jo9n7m7J3JiwrhpCRNioBL4jzo=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Soenneker.Extensions.Long.wasm",
        "name": "Soenneker.Extensions.Long.p7h0lpdvzr.wasm",
        "hash": "sha256-JSDhNcQ7t953QSfKedW3nNlMVKfozNychHjzeFkx3J0=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Soenneker.Extensions.Spans.Bytes.wasm",
        "name": "Soenneker.Extensions.Spans.Bytes.29od0jkg38.wasm",
        "hash": "sha256-+tcnQoHpBGvbRjDEG2Cz6h/Fq65spnX0NSx89OXT6ek=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Soenneker.Extensions.Spans.Chars.wasm",
        "name": "Soenneker.Extensions.Spans.Chars.rh2xdv0l71.wasm",
        "hash": "sha256-39PkxDXfGKMRVFLw9h3SLJHItV7RN/qMcqFTD3G3bhc=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Soenneker.Extensions.Spans.Readonly.Bytes.wasm",
        "name": "Soenneker.Extensions.Spans.Readonly.Bytes.hrjaoxlj5d.wasm",
        "hash": "sha256-czGuphLf3lyDx1A59RytHzOPRN4WWls+CjIuM7+80Xk=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Soenneker.Extensions.Spans.Readonly.Chars.wasm",
        "name": "Soenneker.Extensions.Spans.Readonly.Chars.mbsh937uvu.wasm",
        "hash": "sha256-oi7PGiuIiF9xyzMdWa7ITYG5JcFWfjqiNA3Yp6XbenU=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Soenneker.Extensions.Stream.wasm",
        "name": "Soenneker.Extensions.Stream.28ucj10h20.wasm",
        "hash": "sha256-/gRxvkEOmHqHG243I8XQUuRtAEWjn0Q3cSTOzhg7MXc=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Soenneker.Extensions.String.wasm",
        "name": "Soenneker.Extensions.String.09zbb0226l.wasm",
        "hash": "sha256-zO34Pjl38maYGBEKaAFMogKDroDyBHOuDP+hS8YXUow=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Soenneker.Extensions.Task.wasm",
        "name": "Soenneker.Extensions.Task.o7z3vh4ipl.wasm",
        "hash": "sha256-/AckQyrL1mDCGoU6nDicy/vI6HbUtVwzTS/ACcP04DE=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Soenneker.Extensions.ValueTask.wasm",
        "name": "Soenneker.Extensions.ValueTask.k7zcxm7yot.wasm",
        "hash": "sha256-QAbLLe9xwr8bCo9diOynuhaVxQWQHio4q+rzRD8YjCM=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Soenneker.Json.OptionsCollection.wasm",
        "name": "Soenneker.Json.OptionsCollection.bvzcqw5b9k.wasm",
        "hash": "sha256-adW2bV8ip3easBwn5m92e9VzlS3UVGBVcz99yN0F9QI=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Soenneker.Lepton.Suite.wasm",
        "name": "Soenneker.Lepton.Suite.i60l5nqkh8.wasm",
        "hash": "sha256-1or255Wn8Des05QyUVLbEmZtIYLOh4cRmFtzwufa14M=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Soenneker.Queues.Intrusive.Abstractions.wasm",
        "name": "Soenneker.Queues.Intrusive.Abstractions.apxqwpdy9j.wasm",
        "hash": "sha256-HqF+dK+YC5FXeBWXGxQ3QW2vUcA6hfIAy8eDuJhj7sg=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Soenneker.Queues.Intrusive.ValueMpsc.wasm",
        "name": "Soenneker.Queues.Intrusive.ValueMpsc.o4g1gcztr2.wasm",
        "hash": "sha256-GMgFmSW+u7LLSW8zqIW4ZBlXA1sznJjFvFN8bpA43HM=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Soenneker.Utils.AsyncSingleton.wasm",
        "name": "Soenneker.Utils.AsyncSingleton.c9n1wt6z3l.wasm",
        "hash": "sha256-AABiHxZkvPTXiZSQZ7pNvWSOH9e0QtGl2LXhKJ8xukU=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Soenneker.Utils.AtomicResources.wasm",
        "name": "Soenneker.Utils.AtomicResources.8fsc97rhhe.wasm",
        "hash": "sha256-5C88G2jtzmEZmjvsSTnICAbWrd9MqhiFelWTOdduMTo=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Soenneker.Utils.CancellationScopes.wasm",
        "name": "Soenneker.Utils.CancellationScopes.8y3nukdaov.wasm",
        "hash": "sha256-wHGMHtbwlQUU3zH8SWEvvjqpfiQoh1ftr2s21MmiY8Q=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Soenneker.Utils.ExecutionContexts.wasm",
        "name": "Soenneker.Utils.ExecutionContexts.uojidd3jy8.wasm",
        "hash": "sha256-LqUyFZzmqFc6JrLVRylhkKRY4xGmqjbNznzVaOMZ4Ss=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Soenneker.Utils.File.wasm",
        "name": "Soenneker.Utils.File.efw9q0jti0.wasm",
        "hash": "sha256-linBFLPO1oOJwOxA0ywdayCJolkTl6T/n06zI4bsNv0=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Soenneker.Utils.Json.wasm",
        "name": "Soenneker.Utils.Json.sogngzj3yw.wasm",
        "hash": "sha256-xN/1nukdnlXRRS5qXqMfS8GwQTvpphtyD4Wt06wb0cE=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Soenneker.Utils.MemoryStream.wasm",
        "name": "Soenneker.Utils.MemoryStream.21lxp1s1ba.wasm",
        "hash": "sha256-tkW6GVu0eC1DCW+/Pqzpn/b2gWOeRtlpbU90wCiEPx0=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Soenneker.Utils.PooledStringBuilders.wasm",
        "name": "Soenneker.Utils.PooledStringBuilders.ms42nn9vrd.wasm",
        "hash": "sha256-OvhLqw8aK3XG1o1ZcGrwie4ZYaZlo8DZ5R+QkpmVxRo=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Soenneker.Utils.Random.wasm",
        "name": "Soenneker.Utils.Random.sjxq5nryay.wasm",
        "hash": "sha256-qMIqGUlOiruCZoqHR9fVhdQ6CVfsHl0q7ElZkUeNo6c=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Soenneker.Utils.Runtime.wasm",
        "name": "Soenneker.Utils.Runtime.3x9gg4l5lx.wasm",
        "hash": "sha256-yTkTQgnVHHaIWeH9B+um9s3WNgF33Z/stketRtv5ayg=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.IdentityModel.Tokens.Jwt.wasm",
        "name": "System.IdentityModel.Tokens.Jwt.c85w39uoud.wasm",
        "hash": "sha256-hJYw8JR92cGAIuBWVlmDw64NPfyac7iT6l3T4ilq2rc=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Reactive.wasm",
        "name": "System.Reactive.d1fws3h4o7.wasm",
        "hash": "sha256-31OeEDYnGVfsT/mJsAJjs59Yt6lUXKNj/D/K1z09tf4=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "TinyMCE.Blazor.wasm",
        "name": "TinyMCE.Blazor.3bixcwg00m.wasm",
        "hash": "sha256-7+YouHCo/zl6hkLQJ21cKsQ2NtHiaxJ4RzZ/1/7BmtI=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "YamlDotNet.wasm",
        "name": "YamlDotNet.r7gbnw681s.wasm",
        "hash": "sha256-133XaF/ZcOMivt09C1hHYn1uLhqTP/LjFZ6Xy6fcnSM=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "zxing.wasm",
        "name": "zxing.hfkt0fwhm0.wasm",
        "hash": "sha256-rej+eTXC4W5rfRfJtgmOOtiRU7CFKA0O50IoNI34f/E=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.CSharp.wasm",
        "name": "Microsoft.CSharp.l9qyjq59b2.wasm",
        "hash": "sha256-eeTikqmiucc9fV041AZcqQRyEh1lmW+VwDDzgdOZc4Y=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Collections.Concurrent.wasm",
        "name": "System.Collections.Concurrent.l7m7189014.wasm",
        "hash": "sha256-Mb0cjqzxhnU4wIWSJzWcrp+6yM5lFq+fIgSDJcEBFN0=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Collections.Immutable.wasm",
        "name": "System.Collections.Immutable.asjwmalhgv.wasm",
        "hash": "sha256-Kvrf8xqzy3czuYT9i7ptmNoVldz852JXewn7mJpW5eM=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Collections.NonGeneric.wasm",
        "name": "System.Collections.NonGeneric.1yj2rhtbvl.wasm",
        "hash": "sha256-eUDPYyIZRaMUil3gKJMALqTJT/3XRcodfYBhW3ojquk=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Collections.Specialized.wasm",
        "name": "System.Collections.Specialized.s3ri4xnvag.wasm",
        "hash": "sha256-SM6ejrwUNvXHlJk1SaBYBCY9zuBzQnyHHnhqhVm49dY=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Collections.wasm",
        "name": "System.Collections.09qjmkbfft.wasm",
        "hash": "sha256-XN46E4tPXcP9Vad29Bojl2fs8JiJuROFdrpG0v8Q/HY=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.ComponentModel.Annotations.wasm",
        "name": "System.ComponentModel.Annotations.mg1bocz4z3.wasm",
        "hash": "sha256-JtTlfuNgSKFb3f0NslxqOlPg2YDIYpfq/xdIlJBk4/0=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.ComponentModel.Primitives.wasm",
        "name": "System.ComponentModel.Primitives.ke6s70xvrb.wasm",
        "hash": "sha256-bEbJTeEIMRQ8rPVInntERkwlCongDDfRrzw5vQts8h0=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.ComponentModel.TypeConverter.wasm",
        "name": "System.ComponentModel.TypeConverter.6rns2lgfa6.wasm",
        "hash": "sha256-Qt1/Dsi00/x812/R4nB9YnSAXWgwparszPLFkXSzt0c=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.ComponentModel.wasm",
        "name": "System.ComponentModel.cphsh41vl6.wasm",
        "hash": "sha256-oFm4vMv+Q9oyEejMC9gEG9e7ko8IcrpkPHiy/Dg8F3Q=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Console.wasm",
        "name": "System.Console.k3bp7mbdz4.wasm",
        "hash": "sha256-cHPR8y2kLH5y2cbdNslrwIi+JQoZH+SxzBUzDsJBbeA=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Data.Common.wasm",
        "name": "System.Data.Common.fjdknhlq54.wasm",
        "hash": "sha256-dVO7yvb+3jW22klWUZFOiu9jXtPllwIWZzi/OVBj7G0=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Diagnostics.DiagnosticSource.wasm",
        "name": "System.Diagnostics.DiagnosticSource.yy2bsc9ohq.wasm",
        "hash": "sha256-NkD0U2m08YyGc2dapUseHN6bSx97IPSH+bYSDroXSW4=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Diagnostics.StackTrace.wasm",
        "name": "System.Diagnostics.StackTrace.us3yr3lq9q.wasm",
        "hash": "sha256-QgGkpXpDCTbQuwQztoiguKMu+V0tHMKuahovQz47gSk=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Diagnostics.TraceSource.wasm",
        "name": "System.Diagnostics.TraceSource.aphshkejml.wasm",
        "hash": "sha256-nmlolfxKPDAFnTkongZvpitdKGA2KeO6XxJmy4AAVcY=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Diagnostics.Tracing.wasm",
        "name": "System.Diagnostics.Tracing.0m9mzhgjqt.wasm",
        "hash": "sha256-KIET+Xd45kYy3tfGADe3n3dY83Ck1paVvho4t0ETZSo=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Drawing.Primitives.wasm",
        "name": "System.Drawing.Primitives.jw3iar2ejf.wasm",
        "hash": "sha256-QxpBsfiXSGNrvz4gN8iN1NQNmW/6mtgcN6utgNDM68Q=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Drawing.wasm",
        "name": "System.Drawing.c6tmp1gjc8.wasm",
        "hash": "sha256-NFpXLgCTsyBU+AK+27QTsQQxyAoaLo/HDVh9RRvDIL4=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Formats.Asn1.wasm",
        "name": "System.Formats.Asn1.d8xni0hasz.wasm",
        "hash": "sha256-AxartxaoDE2Y89X4dyTQ5jAwWqS8ZbRuKK6ODS9mzO0=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.IO.Compression.wasm",
        "name": "System.IO.Compression.vithnp6a1d.wasm",
        "hash": "sha256-SfUNI3wylWL1CC4R9BscdENP0UICbXf4odKxskttCwY=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.IO.FileSystem.DriveInfo.wasm",
        "name": "System.IO.FileSystem.DriveInfo.ey9ss13yug.wasm",
        "hash": "sha256-ZhFMqq49zkMIeBfZZpYt8hmWtFiIFSa4e3Z3eO7jie4=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.IO.FileSystem.wasm",
        "name": "System.IO.FileSystem.ucp90zgy2d.wasm",
        "hash": "sha256-Yispoy5A2jW+cn37kA2FevCwSTfIxOj4N1Kc6PrXPWE=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.IO.Pipelines.wasm",
        "name": "System.IO.Pipelines.a2z2jsctze.wasm",
        "hash": "sha256-AgFA5uv6reBqgFFT3HViqqjGZp5tqHulKf2JO8CPfss=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Linq.Expressions.wasm",
        "name": "System.Linq.Expressions.shr8me661y.wasm",
        "hash": "sha256-0BRYJhqdn1b999TRccLi+CDvdpTJLrILjayx9pUIkqw=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Linq.Queryable.wasm",
        "name": "System.Linq.Queryable.20z0a1729h.wasm",
        "hash": "sha256-Mo9erFOK541mj8itp7fbeSoTQYJEtceNL2ravDtnk1I=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Linq.wasm",
        "name": "System.Linq.hchwzcut3i.wasm",
        "hash": "sha256-Lw3svubOyor2NLvCxx4d153l0Du77YZaR0ZtkLTaYFY=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Memory.wasm",
        "name": "System.Memory.hgwx56jr5g.wasm",
        "hash": "sha256-kQ/syT2NBGcI6e3iyzZng1i4fpHXpCccEwnyMCxhWF0=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Net.Http.Json.wasm",
        "name": "System.Net.Http.Json.lzj1uqs3i6.wasm",
        "hash": "sha256-j97ehToT7z3an1cT8aWTH58XV6+0JCYY6NMcyHMF3nc=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Net.Http.wasm",
        "name": "System.Net.Http.24ih7oty8o.wasm",
        "hash": "sha256-DnNPptLzZxinPotVVpgS7dldSOmaRX0/zFaXk7WWBx4=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Net.Primitives.wasm",
        "name": "System.Net.Primitives.lkg1qaxwe0.wasm",
        "hash": "sha256-JdxMMUu3BYdpjk+ynL4pfvkQxMxn0hD+qKYwHQiUnFw=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Net.Requests.wasm",
        "name": "System.Net.Requests.qe2bj2grrz.wasm",
        "hash": "sha256-DiRTfvlQVM63vCaye0I86N/HqejMthMR3K+zUS/9jL8=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Net.Security.wasm",
        "name": "System.Net.Security.rqrp9ih26o.wasm",
        "hash": "sha256-TeakiUBwm9fXtBnK+f2luf8YUZg521uabv9vCmcgcTs=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Net.WebHeaderCollection.wasm",
        "name": "System.Net.WebHeaderCollection.h9w49fqrop.wasm",
        "hash": "sha256-q95xvWqlwv3pzKV207d/x6IiS6z+bbG2asCfT1SdjZ0=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Net.WebProxy.wasm",
        "name": "System.Net.WebProxy.qjy0o7ra7r.wasm",
        "hash": "sha256-5nKabxHIbSBe5L4QBGZmF5KQPB41Hqavt+eL/PO/xR0=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Net.WebSockets.Client.wasm",
        "name": "System.Net.WebSockets.Client.mn888zh3i1.wasm",
        "hash": "sha256-0iH4wLY1Dh923Fq9Six3GXhVWYbJdH+Ok3Td7iBuhlU=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Net.WebSockets.wasm",
        "name": "System.Net.WebSockets.65uksfnu7f.wasm",
        "hash": "sha256-ZgL7mfswOpqhSdNUDOs975CujLteqbosWCjJ8QG09JU=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Numerics.Vectors.wasm",
        "name": "System.Numerics.Vectors.8eqt7jh13n.wasm",
        "hash": "sha256-IPIfVupkJaPuLlhUl8ZTRWUvr7E2pwfw4nX3Lvl2cQM=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.ObjectModel.wasm",
        "name": "System.ObjectModel.xp1upyfg14.wasm",
        "hash": "sha256-yyoJLC3a0Pq/WdSL05RDOUQ3WS+QpntNKDe2a4VZsQk=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Private.Uri.wasm",
        "name": "System.Private.Uri.np27ky7t7f.wasm",
        "hash": "sha256-heomRUE879i7oALWeNz1NSGFjtzCQwJqLf61RHE/k/s=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Private.Xml.Linq.wasm",
        "name": "System.Private.Xml.Linq.38pe2yvw9k.wasm",
        "hash": "sha256-zY7kw0/Rjw/M3w5vRJ0EsJ3ulZ+RbznUSTot6MQb6t0=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Private.Xml.wasm",
        "name": "System.Private.Xml.37svofjhjk.wasm",
        "hash": "sha256-f8SggybxREJEZmuGrL4dwQW1Rg3jznKTewroNu+E1uY=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Reflection.Emit.ILGeneration.wasm",
        "name": "System.Reflection.Emit.ILGeneration.qjk1z49z4z.wasm",
        "hash": "sha256-uv4IRXbCnLjb6RSm68UE1B51lEAjEshWUdwVrAgYVg8=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Reflection.Emit.Lightweight.wasm",
        "name": "System.Reflection.Emit.Lightweight.b1x1ic91s4.wasm",
        "hash": "sha256-fy+wN9mbUBdeCRP1jFWwc8hGeCtD9zAWI/uir0tevn0=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Reflection.Emit.wasm",
        "name": "System.Reflection.Emit.1ofyimt31d.wasm",
        "hash": "sha256-0X1kIKUj4O94kH7wj2djjp5BICfm8R8nEDItRiV+KXs=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Reflection.Primitives.wasm",
        "name": "System.Reflection.Primitives.y0t06ffh5m.wasm",
        "hash": "sha256-DtvUByUM7iugi+iZbOUqRTAFa9/57eM6sEANIyhYeI4=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Resources.ResourceManager.wasm",
        "name": "System.Resources.ResourceManager.9hr4affkg0.wasm",
        "hash": "sha256-48WtQORRYle1yu7mKz0UK+B8bPrEFMKWdBSReowbgK0=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Runtime.InteropServices.RuntimeInformation.wasm",
        "name": "System.Runtime.InteropServices.RuntimeInformation.avus9238sj.wasm",
        "hash": "sha256-ulM4Uuu8EPek9nJ/EznEg8Ko8GXtYWU6d+rjbFBtsWA=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Runtime.InteropServices.wasm",
        "name": "System.Runtime.InteropServices.4otzt7teuq.wasm",
        "hash": "sha256-9In9c15XuZVXkoxFs5k4koKekAyf8asWWnzbyqlDVEM=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Runtime.Numerics.wasm",
        "name": "System.Runtime.Numerics.jxb9ca9r9x.wasm",
        "hash": "sha256-p+gZBsy/D+Jjc7a1hJtI77YjDlexFZXq4lj7r6s2EKY=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Runtime.Serialization.Formatters.wasm",
        "name": "System.Runtime.Serialization.Formatters.vdanj49dzd.wasm",
        "hash": "sha256-U+fJqRnd9Td/HQ7LQ9tHwaLDvYjj+cVdSV5PFwYiJFo=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Runtime.Serialization.Primitives.wasm",
        "name": "System.Runtime.Serialization.Primitives.r84857ekv8.wasm",
        "hash": "sha256-hbcgE9LZWnf8VmquoRecNYH9/sD3RxpdSx2uiXwd164=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Runtime.wasm",
        "name": "System.Runtime.xuohbdijuq.wasm",
        "hash": "sha256-GbeU98V1+K5B6QHT5fnUfx+HXGwRp34jWuN1exVDzl0=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Security.Claims.wasm",
        "name": "System.Security.Claims.2tkmw8xouz.wasm",
        "hash": "sha256-1mJK1EkR3GV/SDpyaq8rkC9thtWJeOVHrqb6PQY/DjE=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Security.Cryptography.Algorithms.wasm",
        "name": "System.Security.Cryptography.Algorithms.ycpyrmoua9.wasm",
        "hash": "sha256-HjjWPxjCczOWexlAt0p+qnj6I7oORGlXuO1+BH8Nm/4=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Security.Cryptography.Csp.wasm",
        "name": "System.Security.Cryptography.Csp.f2i2utptz7.wasm",
        "hash": "sha256-CfZzrqU9sOiwa9CeBFG99KC/gJVMecZiZ5MxIl5m5nY=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Security.Cryptography.wasm",
        "name": "System.Security.Cryptography.16wcqmq4tv.wasm",
        "hash": "sha256-2T83E74LUmkDGIbbORxvZM5J/tclfrKURuE34T1YrL0=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Text.Encoding.CodePages.wasm",
        "name": "System.Text.Encoding.CodePages.dxq87920z7.wasm",
        "hash": "sha256-r3ppfJDL2MCuEll6cNEh6D3rngcGu5eglapomBt66u0=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Text.Encoding.Extensions.wasm",
        "name": "System.Text.Encoding.Extensions.a120pvr2mn.wasm",
        "hash": "sha256-Ts+fI+Xhz2tKuIWDt5/E/FUnfqUEqytumfhGmQRq+1Q=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Text.Encodings.Web.wasm",
        "name": "System.Text.Encodings.Web.30b7n6thw5.wasm",
        "hash": "sha256-c5IdqkfZN0wSLO9q5M3wYZrDd0x1pko+SflTRo0CDBE=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Text.Json.wasm",
        "name": "System.Text.Json.rhii2xgsvx.wasm",
        "hash": "sha256-H5RCcEj6ACRlV9abUUXoywqJXd5g0KB35mUvRUKDcfc=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Text.RegularExpressions.wasm",
        "name": "System.Text.RegularExpressions.ctchgm9ku6.wasm",
        "hash": "sha256-4OyUm2zDAwyK++8exWgmlSPqmyEacg5+jvEpav2dPUM=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Threading.Tasks.Parallel.wasm",
        "name": "System.Threading.Tasks.Parallel.b17w6x7k3p.wasm",
        "hash": "sha256-YboiQ0irxHnq+w2tIMXGzFwO+HUOyBWW7TF1W0D9gcM=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Threading.Thread.wasm",
        "name": "System.Threading.Thread.5ry1asi5th.wasm",
        "hash": "sha256-z/9sp9813ITXlJUi0QmOUPMRNX77BVzO6yU0bIKss/w=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Threading.ThreadPool.wasm",
        "name": "System.Threading.ThreadPool.znmwd0iw9l.wasm",
        "hash": "sha256-v4eIBpEUbK0ezMzw8S+g0ZEObBx8YOpoOg2QrqZF6FI=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Threading.wasm",
        "name": "System.Threading.4km7mp6avo.wasm",
        "hash": "sha256-9CfL+gzZHMFCm/zu+5rsyuyKG1A+5UNsdzYhd3vyE+c=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Web.HttpUtility.wasm",
        "name": "System.Web.HttpUtility.2i03iqplwy.wasm",
        "hash": "sha256-Su9vKuJsmeFUc3TuVxHykaDF/3XkMEMgKDsQRDB+lQs=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Xml.Linq.wasm",
        "name": "System.Xml.Linq.pzw688hdq1.wasm",
        "hash": "sha256-e5txtle0V/jlZ+5laxHe+k2o2xewHh66AVpDrj8yXlc=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Xml.ReaderWriter.wasm",
        "name": "System.Xml.ReaderWriter.60incubrr8.wasm",
        "hash": "sha256-5u4U2mzdmWkA1Uv5B0Xp22D0324dcv+d7FbgZLCOMGI=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Xml.XDocument.wasm",
        "name": "System.Xml.XDocument.urib3r6obo.wasm",
        "hash": "sha256-Nj3gxuEVTBGdvIJ2SEji5A2MVWBj4h1nV6dG5BJ7pNA=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Xml.XPath.wasm",
        "name": "System.Xml.XPath.b0hsk5rr2b.wasm",
        "hash": "sha256-G31PR++E3TMIxaPe6UlANGB7wcBbwQM/T787QLXckT4=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.wasm",
        "name": "System.400l3mulw9.wasm",
        "hash": "sha256-vPEm2KqgXqruGwnz718iSgmDoB8Img7iH3FzO57b0Rg=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "netstandard.wasm",
        "name": "netstandard.t0zxw5in60.wasm",
        "hash": "sha256-jWOuG46cfutmRrcJuB5j7LwWnLn27tBRWV09rfsMFjk=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "BlazorWasmPortfolioGhAction.wasm",
        "name": "BlazorWasmPortfolioGhAction.i5a14114m6.wasm",
        "hash": "sha256-Wt5XkZxBCg/Goumf4p8CjTzkJ2+INY8JUsKdUGEOh8Y=",
        "cache": "force-cache"
      }
    ],
    "satelliteResources": {
      "en": [
        {
          "virtualPath": "BlazorWasmPortfolioGhAction.resources.wasm",
          "name": "BlazorWasmPortfolioGhAction.resources.1p8khf90nr.wasm",
          "hash": "sha256-+7lzCTg0E57ZU+bpgm8vvDyMtV3w79iH8q6fZj0gPnA=",
          "cache": "force-cache"
        }
      ]
    },
    "libraryInitializers": [
      {
        "name": "_content/TinyMCE.Blazor/TinyMce.Blazor.lib.module.js"
      }
    ],
    "modulesAfterRuntimeReady": [
      {
        "name": "../_content/TinyMCE.Blazor/TinyMce.Blazor.lib.module.js"
      }
    ]
  },
  "debugLevel": 0,
  "linkerEnabled": true,
  "appsettings": [
    "../appsettings.json"
  ],
  "globalizationMode": "all",
  "extensions": {
    "blazor": {}
  },
  "runtimeConfig": {
    "runtimeOptions": {
      "configProperties": {
        "Microsoft.AspNetCore.Components.Routing.RegexConstraintSupport": true,
        "Microsoft.Extensions.DependencyInjection.VerifyOpenGenericServiceTrimmability": true,
        "System.ComponentModel.DefaultValueAttribute.IsSupported": false,
        "System.ComponentModel.Design.IDesignerHost.IsSupported": false,
        "System.ComponentModel.TypeConverter.EnableUnsafeBinaryFormatterInDesigntimeLicenseContextSerialization": false,
        "System.ComponentModel.TypeDescriptor.IsComObjectDescriptorSupported": false,
        "System.Data.DataSet.XmlSerializationIsSupported": false,
        "System.Diagnostics.Debugger.IsSupported": false,
        "System.Diagnostics.Metrics.Meter.IsSupported": false,
        "System.Diagnostics.Tracing.EventSource.IsSupported": false,
        "System.GC.Server": true,
        "System.Globalization.Invariant": false,
        "System.TimeZoneInfo.Invariant": false,
        "System.Linq.Enumerable.IsSizeOptimized": true,
        "System.Net.Http.EnableActivityPropagation": false,
        "System.Net.Http.WasmEnableStreamingResponse": true,
        "System.Net.SocketsHttpHandler.Http3Support": false,
        "System.Reflection.Metadata.MetadataUpdater.IsSupported": false,
        "System.Resources.ResourceManager.AllowCustomResourceTypes": false,
        "System.Resources.UseSystemResourceKeys": true,
        "System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeSupported": true,
        "System.Runtime.InteropServices.BuiltInComInterop.IsSupported": false,
        "System.Runtime.InteropServices.EnableConsumingManagedCodeFromNativeHosting": false,
        "System.Runtime.InteropServices.EnableCppCLIHostActivation": false,
        "System.Runtime.InteropServices.Marshalling.EnableGeneratedComInterfaceComImportInterop": false,
        "System.Runtime.Serialization.EnableUnsafeBinaryFormatterSerialization": false,
        "System.StartupHookProvider.IsSupported": false,
        "System.Text.Encoding.EnableUnsafeUTF7Encoding": false,
        "System.Text.Json.JsonSerializer.IsReflectionEnabledByDefault": true,
        "System.Threading.Thread.EnableAutoreleasePool": false,
        "Microsoft.AspNetCore.Components.Endpoints.NavigationManager.DisableThrowNavigationException": false
      }
    }
  }
}/*json-end*/);export{gt as default,ft as dotnet,mt as exit};
