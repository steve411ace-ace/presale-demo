/* Same SafeMedia + MediaType rules as SurveyApi.cs, used only in the internal Builder. */
window.BuilderMedia={inspect(raw){
 const url=(raw||'').trim();if(!url)return {type:null,error:''};
 let parsed;try{parsed=new URL(url);}catch{}
 const safe=url.startsWith('/uploads/survey-media/')&&!url.includes('..')||url==='/images/onenow-logo.png'||parsed?.protocol==='https:';
 if(!safe)return {type:null,error:'Link media phải là HTTPS hoặc file đã tải lên.'};
 const host=parsed?.hostname;let type=null;
 if(['youtube.com','www.youtube.com','youtu.be','www.youtube-nocookie.com'].includes(host))type='youtube';
 else if(['vimeo.com','www.vimeo.com','player.vimeo.com'].includes(host))type='vimeo';
 else {const clean=url.split('?')[0];const ext=clean.slice(clean.lastIndexOf('.')).toLowerCase();if(['.png','.jpg','.jpeg','.webp'].includes(ext))type='image';else if(['.mp4','.webm'].includes(ext))type='video';}
 return {type,error:type?'':'Link ảnh/video không được hỗ trợ.'};
}};
