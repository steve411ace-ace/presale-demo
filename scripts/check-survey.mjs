import assert from 'node:assert/strict';
import fs from 'node:fs';
import vm from 'node:vm';
import {webcrypto} from 'node:crypto';
import {spawnSync} from 'node:child_process';
const read=p=>fs.readFileSync(p,'utf8');let passed=0;
function check(name,fn){fn();passed++;console.log('PASS '+name);}
const index=read('wwwroot/index.html'),server=read('SurveyApi.cs'),publicJs=read('wwwroot/khao-sat.js');
for(const file of ['wwwroot/survey-common.js','wwwroot/khao-sat.js','wwwroot/survey-admin.js'])check('JS syntax: '+file,()=>assert.equal(spawnSync(process.execPath,['--check',file]).status,0));
const inline=[...index.matchAll(/<script(?:\s[^>]*)?>([\s\S]*?)<\/script>/g)].map(x=>x[1]).join('\n');
check('Existing index inline JavaScript syntax',()=>{new vm.Script(inline);});
const context={window:{},URL,crypto:webcrypto,location:{origin:'https://presales.example'},fetch:undefined};vm.createContext(context);vm.runInContext(read('wwwroot/survey-common.js'),context);const ui=context.window.SurveyUI;
check('Upload session tokens work without secure-context randomUUID',()=>{assert.match(ui.randomId(),/^[a-f0-9]{32}$/);assert.notEqual(ui.randomId(),ui.randomId());assert(!publicJs.includes('randomUUID'));});
check('Escapes customer HTML and attribute quotes',()=>assert.equal(ui.escHtml('<img src=x onerror="bad()"> &'), '&lt;img src=x onerror=&quot;bad()&quot;> &amp;'));
check('Summary escapes contact fields',()=>{const html=ui.summary([{id:1,title:'Liên hệ',type:'question_group',fieldsJson:JSON.stringify([{key:'name',label:'Họ tên',type:'short_text'},{key:'email',label:'Email',type:'email'}])}],{1:{name:'<script>alert(1)</script>'}});assert(!html.includes('<script>'));assert(html.includes('Chưa trả lời'));});
check('Rating shows stars and denominator',()=>assert.equal(ui.format({type:'rating',scaleMax:5},3),'★★★ 3/5'));
check('Scale preserves zero',()=>assert.equal(ui.format({type:'scale',scaleMax:10},0),'0/10'));
check('Date shows dd/mm/yyyy',()=>assert.equal(ui.format({type:'date'},'2026-11-06'),'06/11/2026'));
check('Ranking shows ordered and escaped options',()=>assert.equal(ui.format({type:'ranking'},['B','<A>']),'<ol><li>B</li><li>&lt;A></li></ol>'));
check('File summary only shows escaped filename',()=>assert.equal(ui.format({type:'file_upload'},{name:'<file>.pdf',size:123}),'&lt;file>.pdf'));
check('Empty optional answer has readable fallback',()=>assert.equal(ui.format({type:'short_text'},''),'Chưa trả lời'));
check('No media leaves no whitespace',()=>assert.equal(ui.media(null,null),''));
check('Rejects javascript and external HTTP media URLs',()=>{assert.equal(ui.media('javascript:alert(1)','image'),'');assert.equal(ui.media('http://evil.example/a.png','image'),'');});
check('YouTube embeds exact trusted host without autoplay',()=>{const html=ui.media('https://youtu.be/abcdefghijk','youtube');assert(html.includes('youtube-nocookie.com/embed/abcdefghijk'));assert(!html.includes('autoplay'));assert.equal(ui.media('https://youtube.com.evil.example/watch?v=abcdefghijk','youtube'),'');});
check('Vimeo embeds numeric ID without autoplay',()=>assert(ui.media('https://vimeo.com/123456','vimeo').includes('player.vimeo.com/video/123456')));
check('File videos have controls without autoplay',()=>{const html=ui.media('/uploads/survey-media/clip.mp4','video');assert(html.includes('controls'));assert(!html.includes('autoplay'));});
check('Seed has 10 active steps and 11 inactive suggestions',()=>{const seed=server.split('public static List<SurveyQuestion> Defaults()')[1].split('public static async Task Initialize')[0];const calls=[...seed.matchAll(/Q\((\d+),/g)];assert.deepEqual(calls.map(x=>Number(x[1])),Array.from({length:21},(_,i)=>i+1));assert.equal((seed.match(/active:false/g)||[]).length,11);});
check('Contact group has 4 fields; three required stable keys',()=>{const contact=server.split('group.FieldsJson = Encode')[1].split('var result')[0];assert.equal((contact.match(/new SurveyField/g)||[]).length,4);assert.equal((contact.match(/, true\)/g)||[]).length,3);for(const key of ['contactName','contactEmail','companyName','role'])assert(contact.includes('"'+key+'"'));});
check('Exactly 16 backend and admin types',()=>{const backend=server.match(/Types = "([^"]+)"/)[1].split(' ');const admin=read('wwwroot/survey-admin.js').match(/const types='([^']+)'/)[1].split(' ');assert.equal(backend.length,16);assert.deepEqual(backend,admin);});
check('Customer page has no admin scripts, mutation APIs or internal links',()=>{const pub=read('wwwroot/khao-sat.html')+publicJs;assert(!/builder|survey-admin|includeInactive|profileUrl|profileId|scorecard|request-ai|survey-questions\/(reset|reorder)/i.test(pub));assert(!/\bbạn\b/iu.test(pub));assert(!/localStorage/.test(pub));});
check('Progress derives step count from definitions',()=>{assert(publicJs.includes('questions.length'));assert(!/Câu [^`]*\/ 10/.test(publicJs));});
check('11 navigation items and original scroll margin retained',()=>{const nav=index.match(/<nav class="nav">([\s\S]*?)<\/nav>/)[1];assert.equal((nav.match(/<a /g)||[]).length,11);assert(index.includes('scroll-margin-top:132px'));});
check('No old teal in survey or main page',()=>{for(const f of ['wwwroot/index.html','wwwroot/survey.css','wwwroot/survey-admin.css'])assert(!/#0a7a5e/i.test(read(f)));});
check('12 survey summary fields, original-answer snapshot and hidden empty card',()=>{const admin=read('wwwroot/survey-admin.js');const fields=admin.split('const fields=')[1].split(';')[0];assert.equal((fields.match(/\['/g)||[]).length,12);assert(index.includes('id="profileSurvey" style="margin-top:12px" hidden'));assert(admin.includes('response.questionsJson'));assert(server.includes('QuestionsJson=Encode(questions)'));});
check('Shared factory starts score zero and copies no dated actions/follow-ups',()=>{const factory=read('ProfileFactory.cs');assert(factory.includes('new Scorecard { ProfileId = p.Id }'));assert(!factory.includes('db.Actions.Add'));assert(!factory.includes('db.FollowUps.Add'));assert(read('Program.cs').includes('await ProfileFactory.Create(db, dto)'));assert(server.includes('await ProfileFactory.Create(db,new ProfileCreateDto'));});
check('Embedded and standalone schema are identical; existing tables untouched',()=>{const schema=read('sql/SurveySchema.sql').trim();assert.equal(server.split('const string Schema = """\n')[1].split('\n""";')[0].trim(),schema);assert(read('sql/CreateDatabase.sql').includes(schema.split('IF OBJECT_ID(N\'dbo.SurveyPresentations')[0].trim()));assert(!/DROP|ALTER TABLE \[?(Profiles|DiscoveryItems)/i.test(schema));});
check('Contact group protected on server; raw response still saved on duplicate',()=>{assert(server.includes('if(id is 1 or 2 or 10)'));assert(server.includes('Không được bỏ nhóm Thông tin liên hệ'));assert(server.indexOf('db.SurveyResponses.Add(response)')<server.indexOf('return duplicate?Results.Conflict(result)'));});
// Execute the customer controller with a minimal DOM and a mocked transport.
const elements=new Map();
const element=id=>{if(!elements.has(id))elements.set(id,{value:'',textContent:'',innerHTML:'',style:{},classList:{toggle(){},add(){}},reportValidity:()=>true,querySelectorAll:()=>[],querySelector:()=>null});return elements.get(id);};
const doc={getElementById:id=>['value','file'].includes(id)?null:element(id),addEventListener(){}};
let posted;
const controller={SurveyUI:{...ui,api:async(url,method,body)=>{if(method==='POST'){posted=body;return {surveyId:1};}return url.includes('questions')?[]:{};}},document:doc,window:{scrollTo(){}},location:{reload(){}}};
vm.createContext(controller);
const exposed=publicJs.replace(/\n\}\)\(\);\s*$/,`
 globalThis.testSurvey={next,submit,render,choiceOptions,demoSuggestions,set(q,a){questions=q;answers=a;step=0;finished=false;},get step(){return step;}};
})();`);
vm.runInContext(exposed,controller);
await new Promise(resolve=>setImmediate(resolve));
const customer=controller.testSurvey;
const industryQuestion={id:2,type:'single_choice',required:true,optionsJson:JSON.stringify(['Logistic','FnB','Hotel','Sản xuất','Khác'].map(text=>({text})))};
check('Other industry rejects blank, whitespace, one character, numbers and punctuation in next()',()=>{
 for(const value of ['', '   ', 'A', '123', '!!!']){
  customer.set([industryQuestion],{2:'Khác'});element('industry-other').value=value;customer.next();
  assert.equal(customer.step,0);assert.match(element('survey-error').textContent,/nhập ngành của công ty/);
 }
});
check('Other industry accepts and trims Kinh doanh; old industries need no other text',()=>{
 for(const industry of ['Khác','Logistic','FnB','Hotel','Sản xuất']){
  customer.set([industryQuestion],{2:industry});element('industry-other').value=industry==='Khác'?'  Kinh doanh  ':'';customer.next();assert.equal(customer.step,1);
 }
});
check('Other industry renders the text field; four old industries hide it',()=>{
 for(const industry of ['Khác','Logistic','FnB','Hotel','Sản xuất']){
  customer.set([industryQuestion],{2:industry,industryOther:'Kinh doanh'});customer.render();
  assert.equal(element('survey-screen').innerHTML.includes('id="industry-other"'),industry==='Khác');
 }
});
const genericPains=['Chấm công','Tính lương','Xin phép - Nghỉ phép','Tuyển dụng','BHXH','Đánh giá - KPI'];
const suggestions=JSON.parse('['+server.match(/\["Khác"\] = new\[\] \{ ([^}]+) \}/)[1]+']');
check('Other uses six generic pain options and three actual server suggestions',()=>{
 customer.set([],{2:'Khác'});
 assert.deepEqual(Array.from(customer.choiceOptions({id:6,optionsJson:JSON.stringify(genericPains.map(text=>({text}))),industryOptions:{Logistic:['Riêng Logistic']}}),x=>x.text),genericPains);
 assert.equal(suggestions.length,3);
 assert.deepEqual(Array.from(customer.demoSuggestions({industrySuggestions:{Khác:suggestions,Logistic:['Riêng Logistic']}})),suggestions);
 for(const industry of ['Logistic','FnB','Hotel','Sản xuất']){
  customer.set([],{2:industry});
  assert.equal(customer.choiceOptions({id:6,industryOptions:{[industry]:['Riêng '+industry]}})[0].text,'Riêng '+industry);
  assert.equal(customer.demoSuggestions({industrySuggestions:{[industry]:['Demo '+industry],Khác:suggestions}})[0],'Demo '+industry);
 }
});
customer.set([industryQuestion],{2:'Khác',industryOther:'Kinh doanh'});await customer.submit();
check('Submit sends industryOther outside question answer keys',()=>{assert.equal(posted.industryOther,'Kinh doanh');assert.equal(posted.answers[2],'Khác');assert(!Object.hasOwn(posted.answers,'industryOther'));});
customer.set([industryQuestion],{2:'Hotel',industryOther:'Old value'});await customer.submit();
check('Switching back to supported industry does not submit stale other text',()=>assert.equal(posted.industryOther,undefined));
check('Server validates and stores other metadata after strict question-key validation',()=>{
 assert(read('Models.cs').includes('string? IndustryOther = null'));
 assert(server.includes('var industryOther=(dto.IndustryOther??"").Trim();'));
 assert(server.includes('industryOther.Length<2 || !Regex.IsMatch(industryOther,@"\\p{L}")'));
 assert(server.indexOf('answers.Keys.Any')<server.indexOf('answers["industryOther"]='));
 assert(server.indexOf('answers["industryOther"]=')<server.indexOf('AnswersJson=Encode(answers)'));
 assert(!server.includes('ngành Khác chưa được hỗ trợ'));assert(!publicJs.includes('ngành Khác chưa được hỗ trợ'));
 const factory=read('ProfileFactory.cs');assert.equal((factory.match(/new Scorecard/g)||[]).length,1);assert(factory.includes('SeedData.ChecklistTitles()'));
});
check('Profile summary reads stored industryOther and escapes the combined label',()=>{
 const admin=read('wwwroot/survey-admin.js');assert(admin.includes("JSON.parse(s.answersJson||'{}').industryOther"));assert(admin.includes("['Ngành',industry]"));assert(admin.includes("e(value||'Chưa trả lời')"));
});
console.log(`\n${passed} checks passed. Static/JS verification only; no .NET or live API execution.`);
