/* Internal configuration screen; never loaded by the customer survey. */
(()=>{
'use strict';
const {api,escHtml:e,parse,summary,media}=SurveyUI;
const types='short_text long_text single_choice multi_choice dropdown yes_no number email phone date rating scale ranking file_upload picture_choice question_group'.split(' ');
const typeNames=['Điền ngắn','Đoạn dài','Chọn một','Chọn nhiều','Menu thả xuống','Có / Không','Số','Email','Điện thoại','Ngày','Sao','Thang điểm','Xếp hạng','Tải file','Lựa chọn hình','Nhóm câu hỏi'];
let all=[],editing=null,summaryRequest=0,surveyProfiles=new Set();
const dialog=document.createElement('dialog');dialog.className='survey-admin-dialog nopresent';document.body.append(dialog);
const panel=document.createElement('dialog');panel.className='survey-admin-dialog nopresent';document.body.append(panel);
function toast(message){let box=document.querySelector('.survey-toast');if(!box){box=document.createElement('div');box.className='survey-toast';box.setAttribute('role','status');document.body.append(box);}box.textContent=message;clearTimeout(toast.timer);toast.timer=setTimeout(()=>box.remove(),3500);}
async function copy(path){
 const text=new URL(path,location.origin).href;let copied=false;
 try{if(navigator.clipboard){await navigator.clipboard.writeText(text);copied=true;}}catch{}
 if(!copied){const input=document.createElement('textarea');input.value=text;input.style.cssText='position:fixed;opacity:0';document.body.append(input);input.select();try{copied=document.execCommand('copy');}catch{}input.remove();}
 toast(copied?'Đã sao chép link.':'Không thể sao chép. Vui lòng kiểm tra quyền clipboard.');
}
function closeButton(target){target.querySelector('[data-close]').onclick=()=>target.close();}
function showAnswers(response){panel.innerHTML='<button class="btn ghost" data-close>Đóng</button><h2>Toàn bộ câu trả lời khảo sát</h2>'+summary(parse(response.questionsJson),JSON.parse(response.answersJson));closeButton(panel);if(!panel.open)panel.showModal();}
async function refresh(){
 const request=++summaryRequest,p=activeProfile(),box=document.getElementById('profileSurvey');box.hidden=true;
 document.querySelectorAll('#profSelect option').forEach(option=>{if(surveyProfiles.has(Number(option.value))&&!option.textContent.includes('Có câu trả lời khảo sát'))option.textContent+=' · Có câu trả lời khảo sát';});
 if(!p?._serverId)return;
 try{
  const s=await api('/api/profiles/'+p._serverId+'/survey');if(request!==summaryRequest||activeProfile()?._serverId!==p._serverId)return;
  const pain=parse(s.painPointsJson).join(', ');
  const industryOther=JSON.parse(s.answersJson||'{}').industryOther;
  const industry=s.industry==='Khác'&&industryOther?s.industry+' ('+industryOther+')':s.industry;
  const fields=[['Tên công ty',s.companyName],['Ngành',industry],['Người liên hệ',[s.contactName,s.role].filter(Boolean).join(' — ')],['Email liên hệ',s.contactEmail],['Quy mô nhân sự',s.employeeBucket],['Số địa điểm / chi nhánh',s.locationsBucket],['Hệ thống hiện tại',s.currentSystem],['Thời gian áp dụng dự kiến',s.applyTimeline],['Đau nhất đã chọn',pain],['Ca làm việc',s.shiftType],['Người quyết định cuối',s.decisionMaker],['3 điều muốn thấy trong demo',s.demoMustSee]];
  box.innerHTML='<h3>Thông tin từ khảo sát trước demo</h3><span class="survey-badge">Có câu trả lời khảo sát</span><dl>'+fields.map(([label,value])=>'<dt>'+e(label)+'</dt><dd>'+e(value||'Chưa trả lời')+'</dd>').join('')+'</dl><div class="actions"><button class="btn ghost small" id="viewSurvey">Xem câu trả lời</button> <button class="btn ghost small" id="allSurvey">Xem toàn bộ câu trả lời khảo sát</button> <button class="btn small" id="copyProfileSurvey">Sao chép link hồ sơ</button></div>';
  box.hidden=false;document.getElementById('viewSurvey').onclick=document.getElementById('allSurvey').onclick=()=>showAnswers(s);document.getElementById('copyProfileSurvey').onclick=()=>copy('/?profile='+p._serverId);
 }catch(err){if(err.status!==404&&request===summaryRequest)toast('Chưa tải được câu trả lời khảo sát.');}
}
async function load(){all=await api('/api/survey-questions?includeInactive=true');}
async function run(task){try{await task();}catch(err){toast(err.message);}}
function optionsSelect(selected,allowed=types){return allowed.map(t=>'<option value="'+t+'" '+(selected===t?'selected':'')+'>'+e(typeNames[types.indexOf(t)])+'</option>').join('');}
function home(){
 editing=null;const active=all.filter(q=>q.isActive),inactive=all.filter(q=>!q.isActive);
 dialog.innerHTML='<div class="actions"><button class="btn ghost" data-close>Đóng</button><button class="btn ghost" id="restoreSurvey">Khôi phục câu gốc</button></div><h2>Cấu hình khảo sát trước demo</h2><div id="intro-config"></div><div id="question-list">'+active.map((q,i)=>'<div class="question-row"><b>'+ (i+1)+'. '+e(q.title)+'</b><p>'+e(typeNames[types.indexOf(q.type)])+' · '+parse(q.optionsJson).length+' đáp án'+(q.required?' · Bắt buộc':'')+'</p><div class="actions"><button class="btn ghost" data-up="'+q.id+'" '+(!i?'disabled':'')+'>↑</button><button class="btn ghost" data-down="'+q.id+'" '+(i===active.length-1?'disabled':'')+'>↓</button><button class="btn" data-edit="'+q.id+'">Sửa</button><button class="btn ghost" data-delete="'+q.id+'" '+([1,2,10].includes(q.id)?'disabled':'')+'>Xóa</button></div></div>').join('')+'</div><h3>Thêm câu hỏi mới</h3><div id="new-question"></div><h3>Câu hỏi gợi ý — bấm để thêm</h3>'+inactive.map(q=>'<div class="question-row">'+e(q.title)+' <button class="btn ghost" data-activate="'+q.id+'">+ Thêm</button></div>').join('');
 closeButton(dialog);
 document.getElementById('restoreSurvey').onclick=()=>run(async()=>{if(!confirm('Khôi phục bộ 10 bước gốc? Câu trả lời cũ vẫn được giữ.'))return;await api('/api/survey-questions/reset','POST',{});await load();home();});
 dialog.querySelectorAll('[data-edit]').forEach(b=>b.onclick=()=>edit(all.find(q=>q.id===Number(b.dataset.edit))));
 dialog.querySelectorAll('[data-delete]').forEach(b=>b.onclick=()=>run(async()=>{await api('/api/survey-questions/'+b.dataset.delete,'DELETE');await load();home();}));
 dialog.querySelectorAll('[data-activate]').forEach(b=>b.onclick=()=>run(async()=>{const q=all.find(x=>x.id===Number(b.dataset.activate));q.isActive=true;q.sortOrder=Math.max(0,...all.filter(x=>x.isActive).map(x=>x.sortOrder))+1;await api('/api/survey-questions/'+q.id,'PUT',q);await load();home();}));
 for(const attr of ['up','down'])dialog.querySelectorAll('[data-'+attr+']').forEach(b=>b.onclick=()=>run(async()=>{const ids=active.map(q=>q.id),i=ids.indexOf(Number(b.dataset[attr])),j=i+(attr==='up'?-1:1);[ids[i],ids[j]]=[ids[j],ids[i]];await api('/api/survey-questions/reorder','POST',ids);await load();home();}));
 const fresh={title:'',hint:'',type:'short_text',optionsJson:'[]',fieldsJson:'[]',placeholder:'',required:false,isActive:true,mediaUrl:'',minValue:null,maxValue:null,scaleMax:null};
 editor(document.getElementById('new-question'),fresh,false);
 renderIntro();
}
function edit(q){editing=q.id;dialog.innerHTML='<button class="btn ghost" id="builder-back">← Danh sách câu hỏi</button><h2>Sửa câu hỏi</h2><div id="edit-question"></div>';document.getElementById('builder-back').onclick=home;editor(document.getElementById('edit-question'),JSON.parse(JSON.stringify(q)),true);}
function editor(host,q,isEdit){
 let opts=parse(q.optionsJson),fields=parse(q.fieldsJson);
 host.innerHTML='<form class="question-editor"><label>Loại câu hỏi<select name="type" '+([1,2,10].includes(q.id)?'disabled':'')+'>'+optionsSelect(q.type)+'</select></label><label>Tiêu đề<input name="title" required value="'+e(q.title)+'"></label><label>Description (optional)<textarea name="hint">'+e(q.hint)+'</textarea></label><label><input type="checkbox" name="required" '+(q.required?'checked':'')+' '+([1,2,10].includes(q.id)?'disabled':'')+'> Bắt buộc</label><div data-config></div><h4>Hình / Video minh họa</h4><label>Dán link ảnh hoặc YouTube / Vimeo / mp4 / webm<input name="mediaUrl" type="text" value="'+e(q.mediaUrl||'')+'"></label><p data-media-error role="alert" class="crm-overdue"></p><input name="mediaFile" type="file" accept=".jpg,.png,.webp,.mp4,.webm"><p class="hint">Ảnh tối đa 10 MB; video tối đa 50 MB.</p><button type="button" class="btn ghost" data-clear-media>Xóa media</button><div data-preview>'+media(q.mediaUrl,q.mediaType)+'</div><p data-error role="alert"></p><button class="btn primary" type="submit">'+(isEdit?'Lưu câu hỏi':'Thêm vào cuối')+'</button></form>';
 const form=host.querySelector('form'),config=form.querySelector('[data-config]');
 const fieldSelect=f=>optionsSelect(f.type,['short_text','email','phone','number','date']);
 function captureConfig(){
  if(form.querySelector('[data-options]'))opts=[...form.querySelectorAll('[data-option]')].map(row=>({text:row.querySelector('[data-text]').value,icon:row.querySelector('[data-icon]')?.value||null}));
  if(form.querySelector('[data-fields]'))fields=[...form.querySelectorAll('[data-field]')].map(row=>({key:row.dataset.field,label:row.querySelector('[data-label]').value,type:row.querySelector('[data-type]').value,placeholder:row.querySelector('[data-placeholder]').value,required:row.querySelector('[data-required]').checked}));
  for(const key of ['placeholder','minValue','maxValue','scaleMax']){const input=config.querySelector('[name='+key+']');if(input)q[key]=key==='placeholder'?input.value:input.value===''?null:Number(input.value);}
 }
 function draw(){
  const type=form.elements.type.value;q.type=type;
  const choices=['single_choice','multi_choice','dropdown','ranking','picture_choice','yes_no'].includes(type);
  let html='';
  if(choices){if(type==='yes_no')opts=[{text:'Có'},{text:'Không'}];html='<div data-options>'+opts.map((o,i)=>'<div class="option-row" data-option><input data-text aria-label="Đáp án" value="'+e(o.text)+'" '+(type==='yes_no'?'readonly':'')+'>'+(type==='picture_choice'?'<input data-icon aria-label="Icon" placeholder="Icon / emoji" value="'+e(o.icon||'')+'">':'')+'<button type="button" class="btn ghost" data-remove-option="'+i+'" '+(type==='yes_no'?'disabled':'')+'>Xóa</button></div>').join('')+'</div>'+(type==='yes_no'?'':'<button type="button" class="btn ghost" data-add-option>+ Đáp án</button>');}
  if(type==='question_group')html='<div data-fields>'+fields.map((f,i)=>{const locked=q.id===1&&['contactName','contactEmail','companyName'].includes(f.key);return '<div class="field-row" data-field="'+e(f.key)+'"><label>Nhãn<input data-label value="'+e(f.label)+'" required></label><label>Loại<select data-type '+(locked?'disabled':'')+'>'+fieldSelect(f)+'</select></label><label>Placeholder<input data-placeholder value="'+e(f.placeholder||'')+'"></label><label><input type="checkbox" data-required '+(f.required?'checked':'')+' '+(locked?'disabled':'')+'> Bắt buộc</label><button class="btn ghost" type="button" data-remove-field="'+i+'" '+(locked?'disabled':'')+'>Xóa ô</button></div>';}).join('')+'</div><button class="btn ghost" type="button" data-add-field>+ Ô nhập</button>';
  if(['short_text','long_text','email','phone'].includes(type))html='<label>Placeholder<input name="placeholder" value="'+e(q.placeholder||'')+'"></label>';
  if(type==='number')html='<label>Giá trị nhỏ nhất<input name="minValue" type="number" value="'+(q.minValue??'')+'"></label><label>Giá trị lớn nhất<input name="maxValue" type="number" value="'+(q.maxValue??'')+'"></label>';
  if(type==='rating'||type==='scale')html='<label>Số mức<input name="scaleMax" type="number" min="1" max="100" value="'+(q.scaleMax||(type==='rating'?5:10))+'"></label>'+(type==='scale'?'<p>Thang điểm bắt đầu từ 0 (hỗ trợ NPS 0–10).</p>':'');
  config.innerHTML=html;
  config.querySelector('[data-add-option]')?.addEventListener('click',()=>{captureConfig();opts.push({text:''});draw();});
  config.querySelectorAll('[data-remove-option]').forEach(b=>b.onclick=()=>{captureConfig();opts.splice(Number(b.dataset.removeOption),1);draw();});
  config.querySelector('[data-add-field]')?.addEventListener('click',()=>{captureConfig();fields.push({key:'field_'+SurveyUI.randomId(),label:'',type:'short_text',placeholder:'',required:false});draw();});
  config.querySelectorAll('[data-remove-field]').forEach(b=>b.onclick=()=>{captureConfig();fields.splice(Number(b.dataset.removeField),1);draw();});
 }
 form.elements.type.onchange=()=>{captureConfig();draw();};draw();
 function preview(){const result=BuilderMedia.inspect(form.elements.mediaUrl.value);form.querySelector('[data-media-error]').textContent=result.error;form.elements.mediaUrl.setCustomValidity(result.error);form.querySelector('[data-preview]').innerHTML=result.error?'':media(form.elements.mediaUrl.value,result.type);return !result.error;}
 form.elements.mediaUrl.oninput=preview;preview();
 form.querySelector('[data-clear-media]').onclick=()=>{form.elements.mediaUrl.value='';form.elements.mediaFile.value='';preview();};
 form.elements.mediaFile.onchange=()=>run(async()=>{const f=form.elements.mediaFile.files[0];if(!f)return;const data=new FormData();data.append('file',f);const button=form.querySelector('[type=submit]');button.disabled=true;try{const uploaded=await api('/api/survey-media','POST',data);form.elements.mediaUrl.value=uploaded.url;preview();}finally{button.disabled=false;}});
 form.onsubmit=event=>{event.preventDefault();if(!preview())return;run(async()=>{captureConfig();q.title=form.elements.title.value.trim();q.hint=form.elements.hint.value;q.required=form.elements.required.checked;q.mediaUrl=form.elements.mediaUrl.value.trim();q.type=form.elements.type.value;q.optionsJson=JSON.stringify(['single_choice','multi_choice','dropdown','ranking','picture_choice','yes_no'].includes(q.type)?opts:[]);q.fieldsJson=JSON.stringify(q.type==='question_group'?fields:[]);const button=form.querySelector('[type=submit]');button.disabled=true;try{await api('/api/survey-questions'+(isEdit?'/'+q.id:''),isEdit?'PUT':'POST',q);await load();home();toast('Đã lưu câu hỏi.');}finally{button.disabled=false;}});};
}
async function renderIntro(){
 const host=document.getElementById('intro-config');if(!host)return;
 try{const settings=await api('/api/survey-presentation');if(!host.isConnected)return;
 host.innerHTML='<details><summary>Hình / Video màn mở đầu</summary><label>Link media<input id="intro-media-url" value="'+e(settings.mediaUrl||'')+'"></label><p id="intro-media-error" role="alert" class="crm-overdue"></p><input type="file" id="intro-media-file" accept=".jpg,.png,.webp,.mp4,.webm"><div id="intro-media-preview">'+media(settings.mediaUrl,settings.mediaType)+'</div><button class="btn" id="intro-save">Lưu media mở đầu</button> <button class="btn ghost" id="intro-clear">Xóa media</button></details>';
 const input=document.getElementById('intro-media-url');function preview(){const result=BuilderMedia.inspect(input.value);document.getElementById('intro-media-error').textContent=result.error;input.setCustomValidity(result.error);document.getElementById('intro-save').disabled=!!result.error;document.getElementById('intro-media-preview').innerHTML=result.error?'':media(input.value,result.type);return !result.error;}
 input.oninput=preview;preview();document.getElementById('intro-clear').onclick=()=>{input.value='';preview();};
 document.getElementById('intro-media-file').onchange=ev=>run(async()=>{const f=ev.target.files[0];if(!f)return;const data=new FormData();data.append('file',f);const result=await api('/api/survey-media','POST',data);input.value=result.url;preview();});
 document.getElementById('intro-save').onclick=()=>run(async()=>{if(!preview())return;await api('/api/survey-presentation','PUT',{mediaUrl:input.value.trim()});toast('Đã lưu media mở đầu.');});
 }catch(err){toast(err.message);}
}
window.SurveyAdmin={refresh};
document.getElementById('copySurveyLink').onclick=()=>copy('/khao-sat.html');
document.getElementById('openSurveyBuilder').onclick=()=>run(async()=>{await load();home();dialog.showModal();});
(async()=>{
 await window.initialProfilesReady;
 try{const surveys=await api('/api/surveys');surveyProfiles=new Set(surveys.map(s=>s.profileId));}catch(err){toast('Chưa tải được danh sách khảo sát.');}
 const param=new URLSearchParams(location.search).get('profile');
 if(param!==null){try{if(!/^\d+$/.test(param)||Number(param)<1)throw new Error('invalid');await openServerProfile(Number(param));applyContext();document.getElementById('discovery').scrollIntoView();}catch(err){state.activeProfileId=null;applyContext();toast('Không tìm thấy hồ sơ hoặc chưa tải được dữ liệu. Vui lòng chọn hồ sơ trong danh sách.');history.replaceState(null,'',location.pathname);document.getElementById('profileCard').scrollIntoView();}}
 refresh();
})();
})();
