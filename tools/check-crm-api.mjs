/* Explicit opt-in integration suite for a LOCAL test instance; creates and deletes temporary profiles. */
import assert from 'node:assert/strict';
const base=process.env.CRM_TEST_BASE;
if(!base||process.env.CRM_TEST_WRITE!=='YES')throw Error('Start a LOCAL test instance, then set CRM_TEST_BASE and CRM_TEST_WRITE=YES. This script writes disposable test data.');
const origin=new URL(base);if(!['localhost','127.0.0.1','[::1]'].includes(origin.hostname))throw Error('Only a local test instance is accepted.');
const created=[];let checks=0;
async function req(path,method='GET',body,expected=200){const isForm=body instanceof FormData;const response=await fetch(new URL(path,base),{method,headers:isForm?{}:{'Content-Type':'application/json'},body:body===undefined?(method==='POST'?'{}':undefined):isForm?body:JSON.stringify(body)});const text=await response.text();assert.equal(response.status,expected,`${method} ${path}: ${text}`);checks++;try{return JSON.parse(text);}catch{return text;}}
const stamp=Date.now(),company='CRM-VERIFY-'+stamp,now=new Date(),future=new Date(Date.now()+86400000).toISOString();
try{
 const list=await req('/api/profiles');assert.ok(Array.isArray(list));
 const profile=await req('/api/profiles','POST',{name:company,industry:'Hotel',locations:1,employeeCount:10},201);created.push(profile.id);const id=profile.id;
 let full=await req('/api/profiles/'+id);assert.equal(full.status,'DangChuanBi');assert.equal(full.hasSurvey,false);
 await req('/api/profiles/'+id+'/status','PUT',{status:'invalid'},400);
 await req('/api/profiles/'+id+'/status','PUT',{status:'DaDemo',demoDate:'nonsense'},400);
 await req('/api/profiles/'+id+'/status','PUT',{status:'DaDemo',demoDate:future,owner:'CRM verification'});
 let interactions=await req('/api/profiles/'+id+'/interactions');assert.ok(interactions.some(i=>i.note==='Đổi trạng thái → Đã demo'));
 await req('/api/profiles/'+id+'/status','PUT',{status:'DaBaoGia'});full=await req('/api/profiles/'+id);assert.equal(full.owner,'CRM verification');assert.ok(full.demoDate);
 for(const query of ['status=DaBaoGia','industry=Hotel','hasSurvey=false','search='+company]){const data=await req('/api/profiles?'+query);assert.ok(data.some(p=>p.id===id));if(query==='industry=Hotel')assert.ok(data.every(p=>p.industry==='Hotel'));}
 await req('/api/profiles/'+id+'/interactions','POST',{type:'Bad',note:'hello'},400);
 await req('/api/profiles/'+id+'/interactions','POST',{type:'GhiChu',note:' '},400);
 const interaction=await req('/api/profiles/'+id+'/interactions','POST',{type:'CuocGoi',note:'Verification call',occurredAt:now.toISOString()});await req('/api/profiles/'+id+'/interactions/'+interaction.id,'DELETE',undefined,204);
 await req('/api/profiles/'+id+'/appointments','POST',{title:'Demo validation',startsAt:future,endsAt:now.toISOString()},400);
 const appointment=await req('/api/profiles/'+id+'/appointments','POST',{title:'Demo verification',startsAt:future});assert.ok((await req('/api/appointments')).some(a=>a.id===appointment.id));
 await req('/api/appointments/'+appointment.id,'PUT',{title:'Demo complete',startsAt:now.toISOString(),status:'DaDienRa'});
 assert.ok((await req('/api/profiles/'+id+'/appointments')).some(a=>a.status==='DaDienRa'));
 for(const [name,size,code] of [['proof.pdf',1024*1024,200],['blocked.exe',10,400],['large.pdf',11*1024*1024,400]]){const data=new FormData();data.append('file',new Blob([new Uint8Array(size)]),name);const result=await req('/api/profiles/'+id+'/attachments','POST',data,code);if(code===200){const attachments=await req('/api/profiles/'+id+'/attachments');assert.ok(attachments.some(a=>a.id===result.id));await req('/api/profiles/'+id+'/attachments/'+result.id,'DELETE',undefined,204);}}
 const month=now.toISOString().slice(0,7),before=await req('/api/reports/summary?month='+month);
 await req('/api/profiles/'+id+'/interactions','POST',{type:'BaoGia',note:'Verification quotation',occurredAt:now.toISOString()});
 await req('/api/profiles/'+id+'/status','PUT',{status:'Chot'});
 const after=await req('/api/reports/summary?month='+month);assert.equal(after.summary.quotations,before.summary.quotations+1);assert.equal(after.summary.won,before.summary.won+1);assert.equal(after.byIndustry.length,4);assert.equal(after.byMonth.length,6);assert.equal(after.summary.winRate,after.summary.won/(after.summary.won+after.summary.lost));
 await req('/api/appointments/'+appointment.id,'DELETE',undefined,204);
 // Original ten-step survey definition. Abort if the local instance has a different required questionnaire.
 const questions=await req('/api/survey-questions');assert.equal(questions.length,10,'Use an unchanged ten-step local questionnaire');
 const surveyName=company+'-Survey';const answers={'1':{contactName:'CRM verifier',contactEmail:'crm-'+stamp+'@example.org',companyName:surveyName,role:''},'2':'Hotel','3':'200–500','4':'2–5','5':'Excel','6':['Chấm công'],'7':'Ca xoay','8':'Ban giám đốc','9':'Trong 3–6 tháng','10':'Kiểm tra luồng khảo sát và xóa hồ sơ'};
 // Resolve choice labels from current definitions instead of relying on punctuation in seeds.
 for(const q of questions){if(q.type==='single_choice'&&q.id!==2){const choices=JSON.parse(q.optionsJson);answers[String(q.id)]=choices[0].text;}if(q.type==='multi_choice'){answers[String(q.id)]=[JSON.parse(q.optionsJson)[0].text];}}
 const survey=await req('/api/surveys','POST',{answers},201);created.push(survey.profileId);assert.equal((await req('/api/profiles/'+survey.profileId)).status,'MoiKhaoSat');assert.ok((await req('/api/profiles?status=MoiKhaoSat&hasSurvey=true')).some(p=>p.id===survey.profileId));
 await req('/api/profiles/'+survey.profileId,'DELETE',undefined,204);created.splice(created.indexOf(survey.profileId),1);assert.ok(!(await req('/api/surveys')).some(s=>s.profileId===survey.profileId));
 console.log(`PASS ${checks} API requests plus assertions`);
}finally{for(const id of created.reverse())await req('/api/profiles/'+id,'DELETE',undefined,204);}
