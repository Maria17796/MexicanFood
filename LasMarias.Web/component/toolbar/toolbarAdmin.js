
export default class ToolbarAdmin {
	//=========================================================================
	#benutzer = null;
	#args = null;

	//=========================================================================
	constructor(args) {
        fetch('./component/toolbar/toolbarAdmin.html')
            .then((r) => {
                if (r.ok) {
				console.log("argsss");
				console.log(args);
                    return r.text();
                } else {
                    throw new Error(r.status + ' ' + r.statusText);
                }
            })
            .then((html) => {
                args.target.innerHTML = html;
                this.#init(args);
            })
            .catch((ex) => {
                args.target.innerHTML = '<div class="alert alert-danger" role="alert">Fehler: ' + ex + '</div>';
            });
    }
	
	//=========================================================================
	#init(args) {
		// const infoBenutzerName = args.target.querySelector('#infoBenutzerName');
		// this.#benutzer = args.benutzer;
		// infoBenutzerName.innerText = args.benutzer.benutzername;
		//------------------------------------------------------------------------
        console.log("args2");
		console.log(args);
        const buttonLogout = args.target.querySelector('#buttonLogout');
		buttonLogout.addEventListener('click',() => {
			args.app.apiGet((r) => {
				args.app.benutzer = null;
				console.log(r);
				sessionStorage.removeItem('rolle');
				window.location.href = '/';
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
		})//buttonLogout.addEventListener
	}
	//=========================================================================
	get benutzer() {
		return this.#benutzer;
	}

	set benutzer(v) {
		this.#benutzer = v;
	}
}