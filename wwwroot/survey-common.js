/* Shared rendering utilities. User content is always escaped before HTML insertion. */
window.SurveyUI=(()=>{
 const escHtml=x=>String(x==null?'':x).replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/"/g,'&quot;');
 const parse=x=>{try{return JSON.parse(x||'[]');}catch{return [];}};
 function randomId(){const bytes=new Uint8Array(16);crypto.getRandomValues(bytes);return Array.from(bytes,x=>x.toString(16).padStart(2,'0')).join('');}
 function media(url,type){
  if(!url)return '';
  let u;try{u=new URL(url,location.origin);}catch{return '';}
  if(!['https:','http:'].includes(u.protocol)||u.protocol==='http:'&&u.origin!==location.origin)return '';
  const src=escHtml(u.href);
  if(type==='image')return `<img class="survey-media" src="${src}" alt="Hình minh họa OneNow" loading="lazy">`;
  if(type==='video')return `<video class="survey-media" src="${src}" controls preload="metadata" playsinline></video>`;
  let embed='';
  if(type==='youtube'&&['youtube.com','www.youtube.com','youtu.be','www.youtube-nocookie.com'].includes(u.hostname)){
   const id=u.hostname==='youtu.be'?u.pathname.slice(1):u.searchParams.get('v')||u.pathname.split('/').pop();
   if(/^[\w-]{11}$/.test(id||''))embed='https://www.youtube-nocookie.com/embed/'+id;
  }
  if(type==='vimeo'&&['vimeo.com','www.vimeo.com','player.vimeo.com'].includes(u.hostname)){
   const id=u.pathname.split('/').pop();if(/^\d+$/.test(id||''))embed='https://player.vimeo.com/video/'+id;
  }
  return embed?`<iframe class="survey-media survey-video" src="${embed}" title="Video minh họa OneNow" allow="fullscreen; picture-in-picture" allowfullscreen loading="lazy"></iframe>`:'';
 }
 function format(q,v){
  if(v==null||v===''||Array.isArray(v)&&!v.length)return 'Chưa trả lời';
  if(q.type==='question_group')return parse(q.fieldsJson).map(f=>escHtml(f.label)+': '+format({type:f.type},v[f.key])).join('<br>');
  if(q.type==='file_upload')return escHtml(v.name||'Chưa trả lời');
  if(q.type==='ranking')return '<ol>'+v.map(x=>'<li>'+escHtml(x)+'</li>').join('')+'</ol>';
  if(Array.isArray(v))return v.map(escHtml).join(', ');
  if(q.type==='rating')return '★'.repeat(Math.max(0,Math.min(100,Number(v))))+' '+escHtml(v)+'/'+(q.scaleMax||5);
  if(q.type==='scale')return escHtml(v)+'/'+(q.scaleMax||10);
  if(q.type==='date'&&/^\d{4}-\d{2}-\d{2}$/.test(v))return v.split('-').reverse().join('/');
  return escHtml(v);
 }
 function summary(questions,answers){return '<dl>'+questions.map(q=>'<dt>'+escHtml(q.title)+'</dt><dd>'+format(q,answers[q.id])+'</dd>').join('')+'</dl>';}
 async function api(url,method='GET',body){
  const r=await fetch(url,{method,headers:body instanceof FormData?{}:{'Content-Type':'application/json'},body:body===undefined?undefined:body instanceof FormData?body:JSON.stringify(body)});
  const text=await r.text();let data;try{data=JSON.parse(text);}catch{data=text;}
  if(!r.ok){const error=new Error(data?.message||data||'Không thể kết nối. Anh/chị vui lòng thử lại.');error.status=r.status;error.data=data;throw error;}return data;
 }
 return {escHtml,parse,media,format,summary,api,randomId};
})();
