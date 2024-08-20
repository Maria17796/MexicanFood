import PageHome from "./index.js";
import PageLogin from "./page-login.js";
import PageRegister from "./page-register.js";
import Toolbar from "../component/toolbar/toolbar.js";
import ToolbarAdmin from "../component/toolbar/toolbarAdmin.js"; 
import PageLogout from "./page-logout.js";
import PagePersonList from "./page-person-list.js";
import PageArtikelList from "./page-artikel-list.js";


export default class Application {
  #apiUrl = "http://localhost:49007/api";
  header = null;
  #footer = null;
  #benutzer = null;
  toolbar = null;

  constructor() {
    const header = document.querySelector("header");
    const main = document.querySelector("main");

    this.toolbar = new Toolbar({args:this,target:header,benutzer:null});
    const toolbarData = sessionStorage.getItem('rolle');
    if (toolbarData) {
      const toolbarOptions = JSON.parse(toolbarData);
      if(toolbarOptions === 3)
        this.toolbar = new ToolbarAdmin({
          app: this,
          target: header,
          benutzer: toolbarOptions
        });
    }
    //====================================================
    window.addEventListener("hashchange", (e) => {
      this.#navigate(location.hash);
    });
    this.apiGet((r) => {
			if (r.success) {
				//console.log('hash: ' + location.hash + ' | benutzer');
				this.benutzer = r.options;
				this.#navigate(location.hash);
			}
			else {
				//console.log('start ohne benutzer');
				this.#navigate('#');
			}
		}, (ex) => {
			//console.log('fehlaaa');
			//console.error(ex);
			this.#navigate('#');
		}, '/page', 'init', null);
		
  } //constructor
  
  formatDate(datePar) {
		if (datePar) {
			const dateFormatter = new Intl.DateTimeFormat(navigator.language, {
				year: 'numeric',
				month: '2-digit',
				day: '2-digit'
			});					
	
			let dateObject = null;
			if (datePar instanceof Date) dateObject = datePar;
			else dateObject = new Date(datePar);
			return dateFormatter.format(dateObject);
		}
		else return null;
	}

  #navigate(queryString) {
	console.log(queryString);
	const main = document.querySelector('main');
	let navTarget = '';
	let args = {
		target: main,
		app: this
	};

let hashParts = queryString.split("?");
if (hashParts.length > 1) {
  navTarget = hashParts[0].substr(1);
  let usp = new URLSearchParams(hashParts[1]);
  for (const [key, value] of usp) {
	args[key] = value;
  }
} 
	else navTarget = queryString.substr(1);

    main.innerHTML = "";
    switch (navTarget) {
      case "login":
        new PageLogin(args);
        break;
      case "logout":
        new PageLogout(args);
        break;
      case "register":
        new PageRegister(args);
        break;
      case "personlist":
        new PagePersonList(args);
        break;
       case "artikellist":
        new PageArtikelList(args);
        break;
      default:
        new PageHome(args);
        break;
       
    }
  }

  apiLogin(successCallback, errorCallback, loginData) {
    fetch(this.#apiUrl + "/person/login", {
      credentials: "include",
      method: "POST",
      body: loginData,
      cache: "no-cache",
    })
      .then((r) => {
        console.log(r);
        if (r.status == 200) {
          // Wandelt den JSON-String in ein JavaScript-Objekt um
          return r.json();
        }
        else throw new Error(r.status + " " + r.statusText);
      })
      .then(successCallback)

      .catch(errorCallback);
  }

   apiRegister(successCallback, errorCallback, registerData){
   fetch(this.#apiUrl + "/person/register", {
    credentials: "include",
    method: "POST",
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(registerData),
    cache: "no-cache",
  })
    .then((r) => {
      console.log(r);
      if (r.status == 200) return r.json();
      else throw new Error(r.status + " " + r.statusText);
    })
    .then(successCallback)

    .catch(errorCallback);
}

  apiGetList(successCallback, errorCallback, url, args) {
    let completeUrl = this.#apiUrl + "/person" + url;
    if (args) {
      let i = 0;
      for (let arg in args) {
        completeUrl += (i == 0 ? "?" : "&") + arg + "=" + args[arg];
        i++;
      }
    }

    fetch(completeUrl, {
      credentials: "include",
      cache: "no-cache",
    })
      .then((r) => {
        if (r.status == 200) return r.json();
        else if (r.status == 401) window.open("#logout", "_self");
        else throw new Error(r.status + " " + r.statusText);
      })
      .then(successCallback)
      .catch(errorCallback);
  }

  apiGet(successCallback, errorCallback, url, id, args) {
    let completeUrl = this.#apiUrl + url + "/" + id;
    if (args) {
      let i = 0;
      for (let arg in args) {
        completeUrl += (i == 0 ? "?" : "&") + arg + "=" + args[arg];
        i++;
      }
    }

    fetch(completeUrl, {
      credentials: "include",
      cache: "no-cache",
    })
      .then((r) => {
        if (r.status == 200) return r.json();
        else if (r.status == 401) window.open("#logout", "_self");
        else throw new Error(r.status + " " + r.statusText);
      })
      .then(successCallback)
      .catch(errorCallback);
  }

  apiSet(successCallback, errorCallback, url, id, daten) {
    fetch(this.#apiUrl + url + (id ? "/" + id : ""), {
      credentials: "include",
      method: id ? "PUT" : "POST",
      body: JSON.stringify(daten),
      cache: "no-cache",
      headers: {
        "Content-Type": "application/json",
      },
    })
      .then((r) => {
        if (r.status == 200) return r.json();
        else if (r.status == 401) window.open("#logout", "_self");
        else throw new Error(r.status + " " + r.statusText);
      })
      .then(successCallback)
      .catch(errorCallback);
  }
   
  apiGetArtikelList(successCallback, errorCallback, url, args) {
    let completeUrl = this.#apiUrl + "/artikel" + url;
    if (args) {
      let i = 0;
      for (let arg in args) {
        completeUrl += (i == 0 ? "?" : "&") + arg + "=" + args[arg];
        i++;
      }
    }

    fetch(completeUrl, {
      credentials: "include",
      cache: "no-cache",
    })
      .then((r) => {
        if (r.status == 200) return r.json();
        else if (r.status == 401) window.open("#logout", "_self");
        else throw new Error(r.status + " " + r.statusText);
      })
      .then(successCallback)
      .catch(errorCallback);
  }

  apiArtikelDateiUpload(successCallback, errorCallback, id, files) {
    let formData = new FormData();
    for (let idx = 0; idx < files.length; idx++) {
      formData.append("f" + idx, files[idx], files[idx].name);
    }

    fetch(this.#apiUrl + "/artikel/" + id + "/datei", {
      credentials: "include",
      method: "POST",
      body: formData,
      cache: "no-cache",
    })
      .then((r) => {
        if (r.status == 200) return r.json();
        else if (r.status == 401) window.open("#logout", "_self");
        else throw new Error(r.status + " " + r.statusText);
      })
      .then(successCallback)
      .catch(errorCallback);
  }

  apiDelete(successCallback, errorCallback, url, id) {
    if (!id) errorCallback("ID fehlt!!!");
    else {
      fetch(this.#apiUrl + url + "/" + id, {
        credentials: "include",
        method: "DELETE",
        cache: "no-cache",
      })
        .then((r) => {
          if (r.status == 200) return r.json();
          else if (r.status == 401) window.open("#logout", "_self");
          else throw new Error(r.status + " " + r.statusText);
        })
        .then(successCallback)
        .catch(errorCallback);
    }
  }
}
