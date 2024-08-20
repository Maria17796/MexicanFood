export default class Footbar {
	//===========================================================================================

	//===========================================================================================
	constructor(args) {

		fetch('./component/footer/footbar.html').then( (r) => {
			if (r.status == 200) return r.text();
			else throw new Error (r.status + ' ' + r.statusText);
		}).then( (html) => {
			args.target.innerHTML = html;
			this.#init(args);
		}).catch( (ex) => {
			args.target.innerHTML = '<div class="alert alert-danger" role="alert">Fehler: ' + ex + '</div>';
		});


	}

	//===========================================================================================
	// private
	//===========================================================================================
	#init(args) {

		const containerStandard = args.target.querySelector('#containerStandard');
		const containerTool = args.target.querySelector('#containerTool');
		const colTools = args.target.querySelector('#colTools');

		if (args.items) {
			containerTool.classList.remove('d-none');
			// item = {
			//	text: 'text',
			//  class: 'class',
			//  icon: 'icon',
			// 	click: function
			//}
			let html = '<div class="row">';
			let idx = 0;
			for (let item of args.items) {
				html += `<div class="col-12 col-lg-2"><button class="btn w-100 ${(item.class ? item.class : '')}${idx > 0 ? ' mt-2 mt-lg-0': ''}" data-idx=${idx}>
					${(item.icon ? `<iconify-icon icon="bi:${item.icon}" class="fs-5${(item.text ? ' me-2' : '')}"></iconify-icon>` : '')}
					${(item.text ? '<span class="fs-5">' + item.text + '</span>' : '')}
				</button></div>`;
				idx++;
			}
			html += '</div>';
			colTools.innerHTML = html;


		}
		else {
			containerStandard.classList.remove('d-none');
		}


		//------------------------------------------------------------
		colTools.addEventListener( 'click', (e) => {
			let btn = e.target;
			while (btn.nodeName != 'BUTTON') btn = btn.parentElement;
			let idx = parseInt(btn.dataset.idx);
			if (args.items[idx].click &&  typeof args.items[idx].click === 'function') args.items[idx].click();
		});



	}

}