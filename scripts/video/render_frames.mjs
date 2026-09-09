// Static source-based preview rendering; does not run ASP.NET or execute DB actions.
import fs from 'node:fs';
import path from 'node:path';
import {fileURLToPath} from 'node:url';
import {chromium} from '../../.video-tools/node_modules/playwright/index.mjs';
import bundled from '../../.video-tools/node_modules/@sparticuz/chromium/build/index.js';
import {inflate} from '../../.video-tools/node_modules/@sparticuz/chromium/build/lambdafs.js';
const root=path.resolve(path.dirname(fileURLToPath(import.meta.url)),'../..');
const work=path.join(root,'.video-work');
await inflate(path.join(root,'.video-tools/node_modules/@sparticuz/chromium/bin/al2023.tar.br'));
process.env.LD_LIBRARY_PATH='/tmp/al2023/lib'+(process.env.LD_LIBRARY_PATH?':'+process.env.LD_LIBRARY_PATH:'');
const scenes=JSON.parse(fs.readFileSync(path.join(work,'scenes.json'),'utf8'));
const browser=await chromium.launch({executablePath:await bundled.executablePath(),args:bundled.args,headless:true});
const page=await browser.newPage({viewport:{width:1920,height:1080},deviceScaleFactor:1});
const esc=s=>s.replaceAll('&','&amp;').replaceAll('<','&lt;').replaceAll('"','&quot;');
for (let i=0;i<scenes.length;i++) {
 const s=scenes[i];
 const source=fs.readFileSync(path.join(work,s.file),'utf8');
 const shell=`<!doctype html><html><head><meta charset="utf-8"><style>
 *{box-sizing:border-box}body{margin:0;background:#112b20;color:#fff;font-family:"DejaVu Sans",Arial,sans-serif} .orb{position:absolute;right:-90px;top:-180px;width:750px;height:750px;border-radius:50%;background:#19392a;z-index:-1}
 .top{position:absolute;left:80px;right:80px;top:35px;display:flex;justify-content:space-between;align-items:center}.eyebrow{font-size:13px;letter-spacing:2.4px;color:#c2d99f;font-weight:700}.badge{font-size:14px;padding:10px 18px;border:1px solid #4b6554;border-radius:30px;color:#d3e0d5;background:#1b382a}
 h1{position:absolute;left:80px;top:68px;margin:0;font-size:36px;font-weight:500;letter-spacing:-1px}.number{position:absolute;right:82px;top:90px;font-size:18px;color:#9eafa0}
 .browser{position:absolute;left:80px;top:154px;width:1760px;height:750px;border:1px solid #658363;border-radius:13px;overflow:hidden;box-shadow:0 22px 65px #0004;background:#fff}.chrome{height:45px;background:#eaf0e3;display:flex;align-items:center;gap:8px;padding:0 20px;color:#52644f;font-size:12px}.dot{width:10px;height:10px;background:#c1cbb7;border-radius:50%}.url{margin-left:35px;background:#f7faf3;border:1px solid #dbe4d3;border-radius:5px;padding:5px 18px;flex:1}.sample{font-size:10px;letter-spacing:1.6px;margin-left:22px;color:#586a50}
 iframe{position:absolute;left:0;top:45px;width:1600px;height:640px;border:0;transform:scale(1.1);transform-origin:top left;background:#fafbf6}
 .bottom{position:absolute;left:80px;right:80px;top:940px;display:flex;gap:22px;align-items:flex-start}.tag{font-size:11px;letter-spacing:1.5px;color:#b9d692;border-top:2px solid #bed985;padding-top:10px;min-width:106px}.caption{font-size:23px;line-height:1.55;color:#f0f4eb;max-width:1530px;margin:-4px 0 0}.footer{position:absolute;bottom:17px;left:80px;right:80px;display:flex;justify-content:space-between;font-size:10px;letter-spacing:1.8px;color:#8fa591}
 </style></head><body><div class="orb"></div><div class="top"><span class="eyebrow">${esc(s.chapter)}</span><span class="badge">SOURCE-BASED UI PREVIEW · SAMPLE DATA</span></div><h1>${esc(s.title)}</h1><span class="number">${String(i+1).padStart(2,'0')} / ${scenes.length}</span><div class="browser"><div class="chrome"><span class="dot"></span><span class="dot"></span><span class="dot"></span><div class="url">FreshBasket / ${esc(s.page)}</div><span class="sample">ILLUSTRATIVE SCREEN</span></div><iframe id="app" title="UI source preview" srcdoc="${esc(source)}"></iframe></div><div class="bottom"><div class="tag">FEATURE TOUR</div><p class="caption">${esc(s.caption)}</p></div><div class="footer"><span>FRESHBASKET / ONLINE GROCERY STORE</span><span>WINDOWS / ACE RUNTIME VERIFICATION PENDING</span></div></body></html>`;
 await page.setContent(shell,{waitUntil:'load'});
 const frame=page.frames().find(f=>f.parentFrame());
 await frame.evaluate(async()=>{await document.fonts.ready;await Promise.all(Array.from(document.images).map(im=>im.decode().catch(()=>{})))});
 await frame.evaluate(({scroll,focus})=>{window.scrollTo(0,scroll);if(focus){const e=document.querySelector(focus);if(e)e.classList.add('video-highlight')}},{scroll:s.scroll,focus:s.focus});
 if(s.focus){
   const box=await frame.locator(s.focus).first().boundingBox();
   // Playwright boxes are already transformed into main-frame viewport coordinates.
   if(box){s.pointer={x:Math.max(105,Math.min(1810,box.x+box.width*.75)),y:Math.max(230,Math.min(875,box.y+Math.min(box.height/2,35)))}};
 }
 await page.screenshot({path:path.join(work,`scene-${String(i).padStart(2,'0')}.png`)});
 if(s.scrollEnd !== undefined){
   await frame.evaluate(y=>window.scrollTo(0,y),s.scrollEnd);
   if(s.focus){const box=await frame.locator(s.focus).first().boundingBox();if(box)s.pointerEnd={x:Math.max(105,Math.min(1810,box.x+box.width*.75)),y:Math.max(230,Math.min(875,box.y+Math.min(box.height/2,35)))}};
   await page.screenshot({path:path.join(work,`scene-${String(i).padStart(2,'0')}-end.png`)});
 }
 console.log(`${i+1}/${scenes.length}: ${s.title}`);
}
fs.writeFileSync(path.join(work,'scenes.json'),JSON.stringify(scenes,null,2));
await browser.close();
