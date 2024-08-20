export default class PageRegister {

    constructor(args) {
        fetch('./src/page-register.html')
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

    #init(args) {
        const btnSave = args.target.querySelector('#btnSave');
        const textVorname = args.target.querySelector('#textVorname');
        const textname = args.target.querySelector('#textName');
        const textBenutzername = args.target.querySelector('#textBenutzername');
        const dateGeburtstagsdatum = args.target.querySelector('#dateGeburtsdatum');
        const textPasswort = args.target.querySelector('#textPasswort');
        const rowMeldung = args.target.querySelector('#rowMeldung');
        const alertMeldung = args.target.querySelector('#alertMeldung');
        const RegisterLabel = args.target.querySelector('#RegisterLabel')
        const RegisterLabel1 = args.target.querySelector('#RegisterLabel')

        btnSave.addEventListener('click', () => {
            let registerData = {
                vorname: textVorname.value,
                name: textname.value,
                benutzername: textBenutzername.value,
                geburtsdatum: dateGeburtstagsdatum.value, // Korrigiert den Key
                passwort: textPasswort.value
            };

            console.log('Daten werden gesendet:', registerData); // Protokolliert die zu sendenden Daten

            args.app.apiRegister((r) => { 
                console.log('Antwort vom Server:', r); // Protokolliert die Antwort vom Server
                if (r.success) {
                    args.app.benutzer = r.options;
                    alert('success')
				 window.open('#login', '_self');
				// Redirect auf die Homepage nach erfolgreichem Login
				window.location.href = '/';
                    // Hier kannst du den Benutzer weiterleiten oder andere Aktionen ausführen
                } else {
                    if (r.message === "Sie sind bereits registriert") {
                        rowMeldung.classList.remove('d-none');
                        alertMeldung.innerHTML = '<strong>Fehler:</strong> ' + r.message;
                    } else {
                        rowMeldung.classList.remove('d-none');
                        alertMeldung.innerHTML = '<strong>Fehler:</strong> ' + r.message; 
                    }
                    setTimeout(() => {
                        rowMeldung.classList.add('d-none');
                    }, 5000);
                }
            }, (ex) => {
                console.error('Fehler beim API-Aufruf:', ex); // Protokolliert Fehler beim API-Aufruf
            }, registerData);
        });
    }
}
