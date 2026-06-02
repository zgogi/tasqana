
class Html
{
	constructor() {
		this.modalValue = null;
	}

	processDropDown(event) {
		this.hideDropDown();
		const dropDownButton = event.target.closest(".w3-dropdown-click");
		const dropDownMenu = event.target.closest(".w3-dropdown-content");
		if ((null != dropDownButton) && (null == dropDownMenu)) {
			this.showDropDown(dropDownButton);
		}
	}

	processAccordion(event) {
		const click = event.target.closest(".accordion-click");
		if (click == null) return;
		const parent = click.closest(".accordion");
		if (parent == null) return;
		const content = parent.querySelector(".accordion-content");
		if (content == null) return;
		if (content.classList.contains("w3-hide"))
			content.classList.remove("w3-hide");
		else
			content.classList.add("w3-hide");
	}

	showDropDown(button) {
		const found = button.getElementsByClassName("w3-dropdown-content");
		if (found.length > 0) {
			found[0].classList.add("w3-show");
		}
	}

	hideDropDown() {
		this._removeClasses("w3-dropdown-content", "w3-show");
	}

	setValue(id, value) {
		const element = document.getElementById(id);
		element.value = value;
	}

	setVisible(elem, show) {
		if (show)
			elem.classList.remove("w3-hide");
		else
			elem.classList.add('w3-hide');
	}

	createMenu(right=false) {
		const node = document.createElement("div");
		node.className = "w3-dropdown-content w3-bar-block w3-border";
		if (right)
			node.style = "right:0";
		return node;
	}

	createMenuItem(title, data, classes="") {
		const node = document.createElement("span");
		node.className = "w3-bar-item w3-button "+classes;
		node.innerText = title;
		for (const [key, value] of Object.entries(data)) {
			node.dataset[key] = value;
		}



		//node.dataset.id = dataId;
		//node.dataset.modal = command;
		return node;
	}

	showError(error) {
		document.getElementById("error-content").innerText = error ?? "Unknown error";
		document.getElementById("error").classList.remove("w3-hide");
	}

	hideError() {
		document.getElementById("error").classList.add("w3-hide");
	}

	_removeClasses(whenClass, deleteClass) {
		const items = document.getElementsByClassName(whenClass);
		for (var i = 0; i < items.length; ++i) {
			items[i].classList.remove(deleteClass);
		}
	}
}

const html = new Html();






