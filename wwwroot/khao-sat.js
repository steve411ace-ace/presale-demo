(()=>{
'use strict';
const {escHtml:e,parse,media,summary,api}=SurveyUI;
const screen=document.getElementById('survey-screen'),error=document.getElementById('survey-error');
let presentation={mediaUrl:'/images/onenow-logo.png',mediaType:'image'};
let questions=[],answers={},step=-1,sending=false,finished=false;
const uploadToken=SurveyUI.randomId();
const inputTypes={short_text:'text',email:'email',phone:'tel',number:'number',date:'date'};
// Industry tables come from the question API; only generic fallbacks live here.
const genericPainPoints=['Chấm công','Tính lương','Xin phép - Nghỉ phép','Tuyển dụng','BHXH','Đánh giá - KPI'];
const genericSuggestions=['Chấm công và tổng hợp công tự động','Tính lương và kiểm tra sai lệch trước khi chốt','Nghỉ phép và duyệt trên điện thoại'];
function industryValues(table){const industry=answers[2];return industry&&industry!=='Khác'&&Object.hasOwn(table||{},industry)?table[industry]:null;}
function choiceOptions(q){return q.id===6?(industryValues(q.industryOptions)?.map(text=>({text}))|| (parse(q.optionsJson).length?parse(q.optionsJson):genericPainPoints.map(text=>({text})))):parse(q.optionsJson);}
function demoSuggestions(q){return industryValues(q.industrySuggestions)||q.industrySuggestions?.['Khác']||genericSuggestions;}
function suggestionPresent(value,text){return value.split('\n').some(line=>line.replace(/^\s*\d+\.\s*/, '').trim()===text);}
function bindSuggestions(q){
 const field=document.getElementById('value'),items=demoSuggestions(q);
 const buttons=screen.querySelectorAll('[data-suggestion]');
 function refresh(){
  answers[q.id]=field.value;
  buttons.forEach(button=>{const added=suggestionPresent(field.value,items[Number(button.dataset.suggestion)]);button.disabled=added;button.classList.toggle('added',added);button.querySelector('.suggestion-status').textContent=added?'Đã thêm':'+';});
  document.getElementById('use-all').disabled=items.every(text=>suggestionPresent(field.value,text));
 }
 function append(indices){
  for(const i of indices){if(suggestionPresent(field.value,items[i]))continue;
   const numbers=Array.from(field.value.matchAll(/^\s*(\d+)\.\s/gm),m=>Number(m[1]));
   const number=Math.max(0,...numbers)+1;
   field.value+=(field.value.length&&!field.value.endsWith('\n')?'\n':'')+number+'. '+items[i];
  }
  refresh();field.focus({preventScroll:true});
 }
 buttons.forEach(button=>button.onclick=()=>append([Number(button.dataset.suggestion)]));
 document.getElementById('use-all').onclick=()=>append([0,1,2]);
 field.addEventListener('input',refresh);refresh();
}
function fail(message){error.textContent=message;}
function input(q,value,id){const attrs=`id="${e(id)}" placeholder="${e(q.placeholder||'Type your answer here...')}" ${q.required?'required':''}`;
 if(q.type==='long_text')return `<textarea ${attrs}>${e(value||'')}</textarea>`;
 return `<input ${attrs} type="${inputTypes[q.type]||'text'}" value="${e(value??'')}" ${q.minValue!=null?'min="'+q.minValue+'"':''} ${q.maxValue!=null?'max="'+q.maxValue+'"':''} ${q.type==='number'?'step="any"':''} ${q.type==='phone'?'inputmode="tel"':''}>`;
}
function render(){
 fail('');
 screen.classList.toggle('survey-centered',step===-1||finished);
 const count=Math.max(1,questions.length);
 document.getElementById('survey-progress').textContent=`Câu ${Math.max(1,Math.min(count,step+1))} / ${count}`;
 document.getElementById('survey-progress-fill').style.width=(finished?100:Math.max(0,step)/count*100)+'%';
 if(step===-1){screen.innerHTML=`${media(presentation.mediaUrl,presentation.mediaType)}<h1>Khảo sát trước buổi demo</h1><p>Chỉ mất 3 phút — giúp chúng tôi chuẩn bị demo đúng vấn đề của anh/chị</p><button class="primary" id="start">Bắt đầu khảo sát</button>`;document.getElementById('start').onclick=()=>{step=0;render();};return;}
 if(step===questions.length){screen.innerHTML='<h1>Kiểm tra câu trả lời của anh/chị</h1>'+summary(questions,answers)+'<div class="actions"><button id="back">Quay lại</button><button class="primary" id="submit">Gửi khảo sát</button></div>';document.getElementById('back').onclick=()=>{step--;render();};document.getElementById('submit').onclick=submit;return;}
 const q=questions[step],opts=choiceOptions(q);
 if(q.id===6&&Array.isArray(answers[q.id]))answers[q.id]=answers[q.id].filter(value=>opts.some(o=>o.text===value));
 const v=answers[q.id];
 let body='';
 if(q.type==='question_group')body=parse(q.fieldsJson).map((f,i)=>`<label class="group-field"><span>${e(f.label)}${f.required?' *':''}</span>${input(f,v?.[f.key],'field-'+i)}</label>`).join('');
 else if(['single_choice','multi_choice','picture_choice','yes_no'].includes(q.type))body=(q.type==='single_choice'?'<p class="hint">Chọn 1 đáp án</p>':q.type==='multi_choice'?'<p class="hint">'+(q.id===6&&industryValues(q.industryOptions)?'Gợi ý cho ngành '+e(answers[2]):'Chọn nhiều đáp án — có thể chọn nhiều mục')+'</p>':'')+'<div class="choices choices--'+q.type+'">'+opts.map((o,i)=>`<button type="button" data-choice="${i}" class="${(Array.isArray(v)?v.includes(o.text):v===o.text)?'selected':''}" aria-pressed="${Array.isArray(v)?v.includes(o.text):v===o.text}">${['single_choice','multi_choice'].includes(q.type)?'<span class="choice-control" aria-hidden="true"></span>':''}${q.type==='picture_choice'?'<span class="choice-icon">'+e(o.icon||'▧')+'</span>':''}<b>${i<6?String.fromCharCode(65+i):i+1}</b> ${e(o.text)}</button>`).join('')+'</div>'+(q.id===2&&v==='Khác'?'<label>Ngành khác của anh/chị'+input({type:'short_text'},answers.industryOther,'industry-other')+'</label>':'');
 else if(q.type==='dropdown')body='<select id="value"><option value="">Chọn...</option>'+opts.map(o=>`<option ${v===o.text?'selected':''}>${e(o.text)}</option>`).join('')+'</select>';
 else if(q.type==='rating'||q.type==='scale'){
  const min=q.type==='rating'?1:0,max=q.scaleMax||(q.type==='rating'?5:10);
  body='<p class="hint">'+(q.type==='rating'?'Chọn số sao':`Chọn từ ${min} đến ${max}`)+'</p><div class="rating">'+Array.from({length:max-min+1},(_,i)=>`<button type="button" data-number="${i+min}" class="${Number(v)===i+min&&v!=null?'selected':''}">${q.type==='rating'?'★ ':''}${i+min}</button>`).join('')+'</div>';
 }else if(q.type==='ranking'){
  const ranked=Array.isArray(v)?v:opts.map(o=>o.text);answers[q.id]=ranked;
  body=ranked.map((x,i)=>`<div class="ranking-row"><span class="ranking-position" aria-label="Thứ hạng ${i+1}">${i+1}</span><span class="ranking-label">${e(x)}</span><button type="button" data-move="${i}" data-dir="-1" ${i===0?'disabled':''} title="Đưa lên" aria-label="Đưa lên: ${e(x)}">↑</button><button type="button" data-move="${i}" data-dir="1" ${i===ranked.length-1?'disabled':''} title="Đưa xuống" aria-label="Đưa xuống: ${e(x)}">↓</button></div>`).join('');
 }else if(q.type==='file_upload')body='<input id="file" type="file" accept=".pdf,.doc,.docx,.xls,.xlsx,.png,.jpg,.jpeg"><p id="file-status">'+e(v?.name||'Tối đa 10 MB/file')+'</p>';
 else body=input(q,v,'value');
 if(q.id===10&&q.type==='long_text')body+='<div class="demo-suggestions"><p class="hint">'+(industryValues(q.industrySuggestions)?'Gợi ý cho ngành '+e(answers[2]):'Gợi ý chung')+' — bấm để thêm vào ô trên</p>'+demoSuggestions(q).map((text,i)=>`<button type="button" class="suggestion-card" data-suggestion="${i}"><span class="suggestion-number">${i+1}</span><span>${e(text)}</span><span class="suggestion-status">+</span></button>`).join('')+'<button type="button" id="use-all">Dùng cả 3 gợi ý</button></div>';
 screen.innerHTML=`${media(q.mediaUrl,q.mediaType)}<h1>${e(q.title)}${q.required?' *':''}</h1>${q.hint?'<p class="hint">'+e(q.hint)+'</p>':''}<form id="answer-form">${body}<div class="actions"><button type="button" id="back">Quay lại</button>${!q.required?'<button type="button" id="skip">Bỏ qua</button>':''}<button type="submit" class="primary" id="next">${nextLabel(q)}</button></div></form><p class="hint">${q.type==='long_text'?'Ctrl+Enter để tiếp':'Enter để tiếp'}</p>`;
 document.getElementById('answer-form').onsubmit=ev=>{ev.preventDefault();next();};
 document.getElementById('back').onclick=()=>{capture();step--;render();};
 const skip=document.getElementById('skip');if(skip)skip.onclick=()=>{delete answers[q.id];step++;render();};
 screen.querySelectorAll('[data-choice]').forEach(button=>button.onclick=()=>choose(Number(button.dataset.choice)));
 screen.querySelectorAll('[data-number]').forEach(button=>button.onclick=()=>{answers[q.id]=Number(button.dataset.number);render();});
 screen.querySelectorAll('[data-move]').forEach(button=>button.onclick=()=>{const a=answers[q.id],i=Number(button.dataset.move),j=i+Number(button.dataset.dir);[a[i],a[j]]=[a[j],a[i]];render();});
 if(q.id===10&&q.type==='long_text')bindSuggestions(q);
 const file=document.getElementById('file');if(file)file.onchange=async()=>{
  if(!file.files[0])return;const f=file.files[0];if(f.size>10*1024*1024){fail('Anh/chị vui lòng chọn file tối đa 10 MB.');return;}
  const data=new FormData();data.append('file',f);file.disabled=true;
  try{const result=await api('/api/surveys/'+uploadToken+'/files','POST',data);answers[q.id]={name:result.name,size:result.size};document.getElementById('file-status').textContent=result.name;fail('');}catch(err){fail(err.message);}finally{file.disabled=false;}
 };
 const first=screen.querySelector('input:not([type=file]),textarea,select');if(first)first.focus({preventScroll:true});
}
function nextLabel(q){const n=q.type==='multi_choice'?(answers[q.id]||[]).length:0;return 'Tiếp tục'+(n?` (đã chọn ${n})`:'');}
function choose(index){
 const q=questions[step],o=choiceOptions(q)[index];if(!o)return;
 if(q.type==='multi_choice'){const values=answers[q.id]||[];answers[q.id]=values.includes(o.text)?values.filter(x=>x!==o.text):[...values,o.text];
  screen.querySelectorAll('[data-choice]').forEach(button=>{const selected=answers[q.id].includes(choiceOptions(q)[Number(button.dataset.choice)].text);button.classList.toggle('selected',selected);button.setAttribute('aria-pressed',String(selected));});
  document.getElementById('next').textContent=nextLabel(q);}
 else{answers[q.id]=o.text;render();}
}
function capture(){const q=questions[step];if(!q)return;
 if(q.type==='question_group'){answers[q.id]=Object.fromEntries(parse(q.fieldsJson).map((f,i)=>[f.key,document.getElementById('field-'+i)?.value.trim()||'']));}
 const field=document.getElementById('value');if(field)answers[q.id]=field.value.trim();
 const other=document.getElementById('industry-other');if(other)answers.industryOther=other.value.trim();
}
function next(){
 if(sending||finished)return;
 const form=document.getElementById('answer-form');if(!form)return;
 capture();const q=questions[step],v=answers[q.id];
 if(!form.reportValidity())return;
 if(q.required&&(v==null||v===''||Array.isArray(v)&&v.length===0)){fail('Anh/chị vui lòng trả lời câu bắt buộc.');return;}
 if(q.id===2&&v==='Khác'){fail('Anh/chị vui lòng chọn 1 trong 4 ngành Logistic, FnB, Hotel, Sản xuất để chuẩn bị demo; ngành Khác chưa được hỗ trợ.');return;}
 if(document.getElementById('file')?.disabled){fail('Anh/chị vui lòng chờ file tải lên xong.');return;}
 if(q.type==='phone'&&v&&!/^\+?[0-9 ()-]+$/.test(v)||q.type==='phone'&&v&&(v.replace(/\D/g,'').length<9||v.replace(/\D/g,'').length>12)){fail('Số điện thoại cần 9–12 chữ số.');return;}
 if(q.type==='question_group')for(const f of parse(q.fieldsJson)){const x=v?.[f.key];
  if(q.id===1&&['contactName','companyName','role'].includes(f.key)&&x){
   if(x.length<2){fail(f.label+': vui lòng nhập ít nhất 2 ký tự.');return;}
   if(!/\p{L}/u.test(x)){fail(f.label+': vui lòng nhập ít nhất 1 chữ cái.');return;}
  }
  if(f.type==='phone'&&x&&(!/^\+?[0-9 ()-]+$/.test(x)||x.replace(/\D/g,'').length<9||x.replace(/\D/g,'').length>12)){fail(f.label+': số điện thoại cần 9–12 chữ số.');return;}}
 step++;render();window.scrollTo({top:0,behavior:'smooth'});
}
async function submit(){
 if(sending)return;sending=true;document.getElementById('submit').disabled=true;fail('');
 const payload=Object.fromEntries(questions.filter(q=>answers[q.id]!=null).map(q=>[q.id,answers[q.id]]));
 try{await api('/api/surveys','POST',{answers:payload,uploadToken});thanks();}
 catch(err){if(err.status===409&&err.data?.surveyId)thanks();else{fail(err.message);if(err.data?.questionId){step=questions.findIndex(q=>q.id===err.data.questionId);render();fail(err.message);}else document.getElementById('submit').disabled=false;}}
 finally{sending=false;}
}
function thanks(){finished=true;screen.classList.add('survey-centered');document.getElementById('survey-progress-fill').style.width='100%';screen.innerHTML='<h1>Cảm ơn anh/chị!</h1><p>Chúng tôi đã nhận câu trả lời và sẽ liên hệ lại để chuẩn bị buổi demo phù hợp với anh/chị.</p>'+summary(questions,answers)+'<button class="primary" id="restart">Làm lại từ đầu</button>';document.getElementById('restart').onclick=()=>location.reload();}
document.addEventListener('keydown',ev=>{
 if(finished||sending||ev.altKey||ev.metaKey)return;
 if(step<0){if(ev.key==='Enter'){ev.preventDefault();step=0;render();}return;}
 if(step>=questions.length)return;
 const tag=document.activeElement.tagName,q=questions[step];
 if(ev.key==='Enter'&&(tag!=='TEXTAREA'||ev.ctrlKey)){if(tag==='BUTTON')return;ev.preventDefault();next();}
 if(!['INPUT','TEXTAREA','SELECT'].includes(tag)&&!ev.ctrlKey&&/^[a-f]$/i.test(ev.key)&&['single_choice','multi_choice','picture_choice','yes_no'].includes(q.type)){ev.preventDefault();choose(ev.key.toUpperCase().charCodeAt(0)-65);}
});
Promise.all([api('/api/survey-questions'),api('/api/survey-presentation')]).then(([data,settings])=>{presentation=settings;questions=data;if(!questions.length)throw new Error('Khảo sát chưa sẵn sàng. Anh/chị vui lòng thử lại sau.');render();}).catch(err=>fail(err.message));
})();
