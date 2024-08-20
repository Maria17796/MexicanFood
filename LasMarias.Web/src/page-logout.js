export default class PageLogout {

	constructor(args) {
//		args.app.header.innerHTML = '';

		fetch('./src/page-logout.html').then( (r) => {
			if (r.status == 200) return r.text();
			else throw new Error (r.status + ' ' + r.statusText);
		}).then( (html) => {
			args.target.innerHTML = html;
			this.#init(args);
		}).catch( (ex) => {
			args.target.innerHTML = '<div class="alert alert-danger" role="alert">Fehler: ' + ex + '</div>';
		});

	} // constructor

	//=========================================================================
	#init(args) {
		const buttonLogout = args.target.querySelector('#buttonLogout');
		buttonLogout.addEventListener('click',() => {
		args.app.apiGet((r) => {
			args.app.benutzer = null;
			console.log(r);
			sessionStorage.removeItem('rolle');
			// let cs = document.cookie.split(';');
			// for (let c in cs) {
			// 	console.log(c);
			// 	if (c.startsWith('logincode=')) {
			// 		console.log(c.split('=')[1]);
			// 	}
			// }
			//document.cookie = "logincode=;samesite=none;secure=true;path=/;max-age=0;expires=" + new Date(0).toUTCString();
		}, (ex) => {
			alert(ex);
		}, '/person', 'logout', null);

		
	} )
	} // #init
}
