
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
		this.removeClasses(".w3-dropdown-content", "w3-show");
	}

	setValue(id, value) {
		const element = document.getElementById(id);
		element.value = value;
	}

	setVisible(elem, show) {
		this.setClass(elem, !show, "w3-hide");
	}

	setClass(elem, value, className) {
		if (value)
			elem.classList.add(className);
		else
			elem.classList.remove(className);
	}

	showError(error) {
		document.getElementById("error-content").innerText = error ?? "Unknown error";
		document.getElementById("error").classList.remove("w3-hide");
	}

	hideError() {
		document.getElementById("error").classList.add("w3-hide");
	}

	addClasses(query, addClass) {
		const items = document.querySelectorAll(query);
		//console.log(query, items);
		for (var i = 0; i < items.length; ++i) {
			items[i].classList.add(addClass);
		}
	}

	removeClasses(query, deleteClass) {
		const items = document.querySelectorAll(query);
		for (var i = 0; i < items.length; ++i) {
			items[i].classList.remove(deleteClass);
		}
	}
}

const html = new Html();






