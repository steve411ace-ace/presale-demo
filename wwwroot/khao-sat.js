(()=>{
'use strict';
const {escHtml:e,parse,media,summary,api}=SurveyUI;
const screen=document.getElementById('survey-screen'),error=document.getElementById('survey-error');
let presentation={mediaUrl:'/images/onenow-logo.png',mediaType:'image'};
let questions=[],answers={},step=-1,sending=false,autoTimer=null,finished=false;
const uploadToken=SurveyUI.randomId();
const inputTypes={short_text:'text',email:'email',phone:'tel',number:'number',date:'date'};
function fail(message){error.textContent=message;}
function input(q,value,id){const attrs=`id="${e(id)}" placeholder="${e(q.placeholder||'Type your answer here...')}" ${q.required?'required':''}`;
 if(q.type==='long_text')return `<textarea ${attrs}>${e(value||'')}</textarea>`;
 return `<input ${attrs} type="${inputTypes[q.type]||'text'}" value="${e(value??'')}" ${q.minValue!=null?'min="'+q.minValue+'"':''} ${q.maxValue!=null?'max="'+q.maxValue+'"':''} ${q.type==='number'?'step="any"':''} ${q.type==='phone'?'inputmode="tel"':''}>`;
}
function render(){
 clearTimeout(autoTimer);fail('');
 const count=Math.max(1,questions.length);
 document.getElementById('survey-progress').textContent=`Câu ${Math.max(1,Math.min(count,step+1))} / ${count}`;
 document.getElementById('survey-progress-fill').style.width=(finished?100:Math.max(0,step)/count*100)+'%';
 if(step===-1){screen.innerHTML=`${media(presentation.mediaUrl,presentation.mediaType)}<h1>Khảo sát trước buổi demo</h1><p>Chỉ mất 3 phút — giúp chúng tôi chuẩn bị demo đúng vấn đề của anh/chị</p><button class="primary" id="start">Bắt đầu khảo sát</button>`;document.getElementById('start').onclick=()=>{step=0;render();};return;}
 if(step===questions.length){screen.innerHTML='<h1>Kiểm tra câu trả lời của anh/chị</h1>'+summary(questions,answers)+'<div class="actions"><button id="back">Quay lại</button><button class="primary" id="submit">Gửi khảo sát</button></div>';document.getElementById('back').onclick=()=>{step--;render();};document.getElementById('submit').onclick=submit;return;}
 const q=questions[step],v=answers[q.id],opts=parse(q.optionsJson);
 let body='';
 if(q.type==='question_group')body=parse(q.fieldsJson).map((f,i)=>`<label class="group-field"><span>${e(f.label)}${f.required?' *':''}</span>${input(f,v?.[f.key],'field-'+i)}</label>`).join('');
 else if(['single_choice','multi_choice','picture_choice','yes_no'].includes(q.type))body='<div class="choices">'+opts.map((o,i)=>`<button type="button" data-choice="${i}" class="${(Array.isArray(v)?v.includes(o.text):v===o.text)?'selected':''}" aria-pressed="${Array.isArray(v)?v.includes(o.text):v===o.text}">${q.type==='picture_choice'?'<span class="choice-icon">'+e(o.icon||'▧')+'</span>':''}<b>${i<6?String.fromCharCode(65+i):i+1}</b> ${e(o.text)}</button>`).join('')+'</div>'+(q.id===2&&v==='Khác'?'<label>Ngành khác của anh/chị'+input({type:'short_text'},answers.industryOther,'industry-other')+'</label>':'');
 else if(q.type==='dropdown')body='<select id="value"><option value="">Chọn đáp án</option>'+opts.map(o=>`<option ${v===o.text?'selected':''}>${e(o.text)}</option>`).join('')+'</select>';
 else if(q.type==='rating'||q.type==='scale'){
  const min=q.type==='rating'?1:0,max=q.scaleMax||(q.type==='rating'?5:10);
  body='<div class="rating">'+Array.from({length:max-min+1},(_,i)=>`<button data-number="${i+min}" class="${Number(v)===i+min&&v!=null?'selected':''}">${q.type==='rating'?'★ ':''}${i+min}</button>`).join('')+'</div>';
 }else if(q.type==='ranking'){
  const ranked=Array.isArray(v)?v:opts.map(o=>o.text);answers[q.id]=ranked;
  body=ranked.map((x,i)=>`<div class="ranking-row"><span>${i+1}. ${e(x)}</span><button data-move="${i}" data-dir="-1" ${i===0?'disabled':''} aria-label="Đưa lên">↑</button><button data-move="${i}" data-dir="1" ${i===ranked.length-1?'disabled':''} aria-label="Đưa xuống">↓</button></div>`).join('');
 }else if(q.type==='file_upload')body='<input id="file" type="file" accept=".pdf,.doc,.docx,.xls,.xlsx,.png,.jpg,.jpeg"><p id="file-status">'+e(v?.name||'Tối đa 10 MB/file')+'</p>';
 else body=input(q,v,'value');
 screen.innerHTML=`${media(q.mediaUrl,q.mediaType)}<h1>${e(q.title)}${q.required?' *':''}</h1>${q.hint?'<p class="hint">'+e(q.hint)+'</p>':''}<form id="answer-form">${body}<div class="actions"><button type="button" id="back">Quay lại</button>${!q.required?'<button type="button" id="skip">Bỏ qua</button>':''}<button type="submit" class="primary">Tiếp →</button></div></form><p class="hint">${q.type==='long_text'?'Ctrl+Enter để tiếp':'Enter để tiếp'}</p>`;
 document.getElementById('answer-form').onsubmit=ev=>{ev.preventDefault();next();};
 document.getElementById('back').onclick=()=>{capture();step--;render();};
 const skip=document.getElementById('skip');if(skip)skip.onclick=()=>{delete answers[q.id];step++;render();};
 screen.querySelectorAll('[data-choice]').forEach(button=>button.onclick=()=>choose(Number(button.dataset.choice)));
 screen.querySelectorAll('[data-number]').forEach(button=>button.onclick=()=>{answers[q.id]=Number(button.dataset.number);render();});
 screen.querySelectorAll('[data-move]').forEach(button=>button.onclick=()=>{const a=answers[q.id],i=Number(button.dataset.move),j=i+Number(button.dataset.dir);[a[i],a[j]]=[a[j],a[i]];render();});
 const file=document.getElementById('file');if(file)file.onchange=async()=>{
  if(!file.files[0])return;const f=file.files[0];if(f.size>10*1024*1024){fail('Anh/chị vui lòng chọn file tối đa 10 MB.');return;}
  const data=new FormData();data.append('file',f);file.disabled=true;
  try{const result=await api('/api/surveys/'+uploadToken+'/files','POST',data);answers[q.id]={name:result.name,size:result.size};document.getElementById('file-status').textContent=result.name;fail('');}catch(err){fail(err.message);}finally{file.disabled=false;}
 };
 const first=screen.querySelector('input:not([type=file]),textarea,select');if(first)first.focus({preventScroll:true});
}
function choose(index){
 const q=questions[step],o=parse(q.optionsJson)[index];if(!o)return;
 if(q.type==='multi_choice'){const values=answers[q.id]||[];answers[q.id]=values.includes(o.text)?values.filter(x=>x!==o.text):[...values,o.text];render();}
 else{answers[q.id]=o.text;render();if(!(q.id===2&&o.text==='Khác'))autoTimer=setTimeout(next,280);}
}
function capture(){const q=questions[step];if(!q)return;
 if(q.type==='question_group'){answers[q.id]=Object.fromEntries(parse(q.fieldsJson).map((f,i)=>[f.key,document.getElementById('field-'+i)?.value.trim()||'']));}
 const field=document.getElementById('value');if(field)answers[q.id]=field.value.trim();
 const other=document.getElementById('industry-other');if(other)answers.industryOther=other.value.trim();
}
function next(){
 clearTimeout(autoTimer);if(sending||finished)return;
 const form=document.getElementById('answer-form');if(!form)return;
 capture();const q=questions[step],v=answers[q.id];
 if(!form.reportValidity())return;
 if(q.required&&(v==null||v===''||Array.isArray(v)&&v.length===0)){fail('Anh/chị vui lòng trả lời câu bắt buộc.');return;}
 if(q.id===2&&v==='Khác'){fail('Anh/chị vui lòng chọn 1 trong 4 ngành Logistic, FnB, Hotel, Sản xuất để chuẩn bị demo; ngành Khác chưa được hỗ trợ.');return;}
 if(document.getElementById('file')?.disabled){fail('Anh/chị vui lòng chờ file tải lên xong.');return;}
 if(q.type==='phone'&&v&&!/^\+?[0-9 ()-]+$/.test(v)||q.type==='phone'&&v&&(v.replace(/\D/g,'').length<9||v.replace(/\D/g,'').length>12)){fail('Số điện thoại cần 9–12 chữ số.');return;}
 if(q.type==='question_group')for(const f of parse(q.fieldsJson)){const x=v?.[f.key];if(f.type==='phone'&&x&&(!/^\+?[0-9 ()-]+$/.test(x)||x.replace(/\D/g,'').length<9||x.replace(/\D/g,'').length>12)){fail(f.label+': số điện thoại cần 9–12 chữ số.');return;}}
 step++;render();window.scrollTo({top:0,behavior:'smooth'});
}
async function submit(){
 if(sending)return;sending=true;document.getElementById('submit').disabled=true;fail('');
 const payload=Object.fromEntries(questions.filter(q=>answers[q.id]!=null).map(q=>[q.id,answers[q.id]]));
 try{await api('/api/surveys','POST',{answers:payload,uploadToken});thanks();}
 catch(err){if(err.status===409&&err.data?.surveyId)thanks();else{fail(err.message);if(err.data?.questionId){step=questions.findIndex(q=>q.id===err.data.questionId);render();fail(err.message);}else document.getElementById('submit').disabled=false;}}
 finally{sending=false;}
}
function thanks(){finished=true;document.getElementById('survey-progress-fill').style.width='100%';screen.innerHTML='<h1>Cảm ơn anh/chị!</h1><p>Chúng tôi đã nhận câu trả lời và sẽ liên hệ lại để chuẩn bị buổi demo phù hợp với anh/chị.</p>'+summary(questions,answers)+'<button class="primary" id="restart">Làm lại từ đầu</button>';document.getElementById('restart').onclick=()=>location.reload();}
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
