
export default class Toolbar {
	//=========================================================================
	#benutzer = null;
	#args = null;

	//=========================================================================
	constructor(args) {
        fetch('./component/toolbar/toolbar.html')
            .then((r) => {
                if (r.ok) {
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
	}
	//=========================================================================
	get benutzer() {
		return this.#benutzer;
	}

	set benutzer(v) {
		this.#benutzer = v;
	}
}