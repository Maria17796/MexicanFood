import Toolbar from '../component/toolbar/toolbar.js';	
import ToolbarAdmin from '../component/toolbar/toolbarAdmin.js';	

export default class PageLogin {

	constructor(args) {
		fetch('./src/page-login.html').then( (r) => {
			if (r.status == 200) return r.text();
			else throw new Error (r.status + ' ' + r.statusText);
		}).then( (html) => {
			args.target.innerHTML = html;
			this.#init(args);
		}).catch( (ex) => {
			args.target.innerHTML = '<div class="alert alert-danger" role="alert">Fehler: ' + ex + '</div>';
		});

	} //constructor    

	/* #wechselToolbar(args, rolle) {
		const header = document.querySelector('header');
		header.innerHTML = ''; // Alte Toolbar entfernen
		if (rolle === 1) {
			console.log("Rolle: " + rolle + " Zu ToolbarAdmin wechseln");
			debugger; // Add this line to pause the debugger at this point
			args.app.toolbar = new ToolbarAdmin({ args: args.app, target: header, benutzer: args.app.benutzer });
			console.log("Rolle: " + rolle + " ToolbarAdmin OK");

		} else {
			console.log("Rolle: " + rolle + " Zu Toolbar wechseln");
			debugger; // Add this line to pause the debugger at this point
			args.app.toolbar = new Toolbar({ args: args.app, target: header, benutzer: args.app.benutzer });
		}
	} */

	#init(args) {
		const buttonAnmelden = args.target.querySelector('#buttonAnmelden');
		const textBenutzername = args.target.querySelector('#textBenutzername');
		const textPasswort = args.target.querySelector('#textPasswort');
		const rowMeldung = args.target.querySelector('#rowMeldung');
		const alertMeldung = args.target.querySelector('#alertMeldung');

		buttonAnmelden.addEventListener('click',() => {
			//alert('yep')
			let loginData = new FormData();
			loginData.append('benutzername', textBenutzername.value);
			loginData.append('passwort', textPasswort.value);
			args.app.apiLogin((r) => {
				// 1. Argument für successCallback, im Erfolsfall aufgerufen
				if (r.success) {
					args.app.benutzer = r.options;
					console.log(args.app.benutzer);
					//alert('Rolle = ' + ' ' + r.options.rolle + ' angemeldet');
					
					// Store the current toolbar in localStorage
					sessionStorage.setItem('rolle', r.options.rolle);

					// Toolbar wechseln
					//this.#wechselToolbar(args, r.options.rolle);

					// Redirect auf die Homepage nach erfolgreichem Login
					window.location.href = '/';
				}
				else {
					alert('error');
					// Remove the stored toolbar from localStorage
					sessionStorage.removeItem('rolle');
					rowMeldung.classList.remove('d-none');
					alertMeldung.innerHTML = '<strong>Fehler:</strong> ' + r.message;
					setTimeout( () => {
						rowMeldung.classList.add('d-none');
					}, 5000);
				}
			}, 
			// 2. Argument für errorCallback
			(ex) => {
				console.error(ex);
			},
			// 3. Argument für loginData
			loginData);
		}) //buttonAnmelden.addEventListener
	}//#init 
}//class
