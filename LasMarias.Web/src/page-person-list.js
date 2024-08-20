
export default class PagePersonList {
	//=========================================================================
	// private vars
	#personList = null;

	//=========================================================================
	constructor(args) {
		fetch('./src/page-person-list.html').then( (r) => {
			if (r.status == 200) return r.text();
			else throw new Error (r.status + ' ' + r.statusText);
		}).then( (html) => {
			args.target.innerHTML = html;
			this.#init(args);
		}).catch( (ex) => {
			args.target.innerHTML = '<div class="alert alert-danger" role="alert">Fehler: ' + ex + '</div>';
		});
	}

	//=========================================================================
	// private methoden
	#init(args) {

		const textFilter = args.target.querySelector('#textFilter');
		const selectSort = args.target.querySelector('#selectSort');
		const buttonSuche = args.target.querySelector('#buttonSuche');

		const tablePerson = args.target.querySelector('#tablePerson>tbody');
		const buttonNeu = args.target.querySelector('#buttonNeu');
		const modalPerson = args.target.querySelector('#modalPerson');
		const dialogPerson = new bootstrap.Modal(modalPerson);

		const textName = args.target.querySelector('#textName');
		const textVorname = args.target.querySelector('#textVorname');
		const textBenutzerName = args.target.querySelector('#textBenutzerName');
		const dateGebDat = args.target.querySelector('#dateGebDat');
		const selectRolle = args.target.querySelector('#selectRolle');

		const buttonSpeichern = args.target.querySelector('#buttonSpeichern')
		let person = null;

		//------------------------------------------------------------------------
		this.#datenLesen(args);

		//------------------------------------------------------------------------
		textFilter.addEventListener( 'keyup', (e) => {
			// if (e.key == 'Enter') {
			// 	this.#datenLesen(args);
			// }
			if (textFilter.value && textFilter.value.length > 1) this.#datenLesen(args);
		});

		//------------------------------------------------------------------------
		buttonSuche.addEventListener( 'click', () => {
			this.#datenLesen(args);
		});

		//------------------------------------------------------------------------
		selectSort.addEventListener( 'change', () => {
			this.#datenLesen(args);
		});

		//------------------------------------------------------------------------
		tablePerson.addEventListener( 'click', (e) =>  {
			
			let btn = null;

			if (e.target.nodeName == 'ICONIFY-ICON' && e.target.parentElement.nodeName == 'BUTTON') btn = e.target.parentElement;
			else if (e.target.nodeName == 'BUTTON') btn = e.target;

			if (btn) {
				if (btn.dataset.aktion == 'del' && btn.dataset.idx) {
					let person = this.#personList[parseInt(btn.dataset.idx)];
					if (confirm('Wollen Sie ' + person.name + ' ' + person.vorname + ' wirklich löschen?')) {
						args.app.apiDelete((r) => {
							if (r.success) {
								this.#datenLesen(args);
							}
							else {
								alert(r.message);
							}

						}, (ex) => {
							alert(ex);
						}, '/person',  person.personid);
					}
				} 
			}
			else {
				if (e.target.nodeName == 'TD' && e.target.dataset.idx) {
					person = this.#personList[parseInt(e.target.dataset.idx)];

					textName.value = person.name ? person.name : '';
					textVorname.value = person.vorname ? person.vorname : '';
					textBenutzerName.value = person.benutzername ? person.benutzername : '';
					dateGebDat.value = person.gebdat ? person.gebdat.split('T')[0] : '';
					selectRolle.value = person.rolle ? person.rolle : '0';
					dialogPerson.show();
				}
			}
		});

		//------------------------------------------------------------------------
		buttonNeu.addEventListener( 'click', () => {
			dialogPerson.show();

			person = {
				personid: null
			};

			textName.value = '';
			textVorname.value = '';
			textBenutzerName.value = '';
			dateGebDat.value = '';
			selectRolle.value = '0';
			textName.classList.remove('is-invalid');
			textVorname.classList.remove('is-invalid');

		});

		//------------------------------------------------------------------------
		buttonSpeichern.addEventListener( 'click', () => {
			let ok2save = true;
			textName.classList.remove('is-invalid');
			textVorname.classList.remove('is-invalid');

			if (!textName.value) {
				ok2save = false;
				textName.classList.add('is-invalid');
			}

			if (!textVorname.value) {
				ok2save = false;
				textVorname.classList.add('is-invalid');
			}

			if (ok2save) {
				person.name = textName.value ? textName.value : null;
				person.vorname = textVorname.value ? textVorname.value : null;
				person.benutzername = textBenutzerName.value ? textBenutzerName.value : null;
				person.gebdat = dateGebDat.value ? dateGebDat.value : null;
				person.rolle = parseInt(selectRolle.value);
	
				args.app.apiSet((r) => {
					if (r.success) {
						dialogPerson.hide();
						this.#datenLesen(args);
					}
					else alert(r.message);
				}, (ex) => {
					alert(ex);
				}, '/person', person.personid, person);
			}
		});
	}

	#datenLesen(args) {
		const tablePerson = args.target.querySelector('#tablePerson>tbody');
		const textFilter = args.target.querySelector('#textFilter');
		const selectSort = args.target.querySelector('#selectSort');
		const infoText = args.target.querySelector('#infoText');
		let idx = 0;

		let pars = {
			sort: selectSort.value
		};

		if (textFilter.value) {
			pars.filter = textFilter.value;
		}


		args.app.apiGetList((r) => {
			if (r.success) {
				this.#personList = r.options;
				let html = '';
				for (let person of this.#personList) {
					html += 
					`<tr>
						<td>
							<button class="btn btn-outline-light" data-aktion="del" data-idx="${idx}"><iconify-icon icon="bi:trash3"></iconify-icon></button>
						</td>
						<td class="element-clickable" data-idx="${idx}">${person.name} ${person.vorname}</td>
						<td class="element-clickable" data-idx="${idx}">${(person.benutzername ? person.benutzername : '')}</td>
						<td class="element-clickable" data-idx="${idx}">${(person.gebdat ? args.app.formatDate(person.gebdat) : '')}</td>
						<td class="element-clickable" data-idx="${idx}">${(person.rolletext ? person.rolletext : '')}</td>
						<td class="element-clickable" data-idx="${idx}">${(person.loginzuletzt ? args.app.formatDate(person.loginzuletzt) : '')}</td>
					</tr>`;
					idx++;
				}
				tablePerson.innerHTML = html;

				if (this.#personList && this.#personList.length > 0) {
					infoText.innerText = this.#personList.length + ' Zeilen gefunden';
				}
				else {
					infoText.innerText = 'Keine Daten zum Anzeigen gefunden!';
				}

			}
			else {
				if (r.code == 401) window.open('#logout', '_self');
			}
			
		}, (ex) => {
			console.error(ex);
		}, '/personenlist', pars);
	}


}