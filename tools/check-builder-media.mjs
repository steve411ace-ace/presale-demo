import fs from 'node:fs';
import vm from 'node:vm';
import assert from 'node:assert/strict';
const context={window:{},URL};vm.createContext(context);
vm.runInContext(fs.readFileSync(new URL('../wwwroot/builder-media.js',import.meta.url),'utf8'),context);
const inspect=context.window.BuilderMedia.inspect;
const cases=[['https://picsum.photos/800/450',null],['https://picsum.photos/800/450.jpg','image'],['https://example.org/a.JPEG?key=x','image'],['https://youtu.be/aqz-KE-bpKQ','youtube'],['https://www.youtube.com/watch?v=aqz-KE-bpKQ','youtube'],['https://player.vimeo.com/video/1','vimeo'],['https://example.org/demo.webm','video'],['https://youtube.com.evil.org/no',null],['http://example.org/a.jpg',null],['/uploads/survey-media/a.png','image'],['/uploads/survey-media/../a.png',null],['/images/onenow-logo.png','image'],['https://example.org/a.jpg#bad',null],['',null]];
for(const [url,type] of cases){const result=inspect(url);assert.equal(result.type,type,url);assert.equal(!!result.error,type===null&&url!=='',url);}
console.log(`PASS ${cases.length} Builder media cases (pure JS; no browser/server)`);
const admin=fs.readFileSync(new URL('../wwwroot/survey-admin.js',import.meta.url),'utf8');
assert.match(admin,/mediaUrl\.oninput=preview/);assert.match(admin,/\[data-media-error\]/);assert.match(admin,/if\(!preview\(\)\)return/);
console.log('PASS inline input feedback + save guard wired in Builder source');
