<script>
	import _ from "lodash";
	import IconButton from "@smui/icon-button";
	import { Row, Cell } from "@smui/data-table";
	import Textfield from "@smui/textfield";
	import { createEventDispatcher } from 'svelte';

	export let strings;
	export let name;
	export let description;
	export let id;

	let disabled = false;
	const dispatch = createEventDispatcher();

	async function save() {
		let update = { "Id": id, "Description": description, "Type": "StringFlag", "Percent": 0, "Strings": strings }
		const options = {
			method: "POST",
			body: JSON.stringify(update),
			headers: {	"Content-Type": "application/json",},
		};

		disabled = true;
		let res = await fetch("https://localhost:5555/veff_internal_api/update", options);
		//let res = await fetch("/veff_internal_api/update", options);
		if (res.ok) {
			dispatch("saved", update)
		} else {
			dispatch("error", {message: "something went bad" })
		}

		disabled = false;
	}

</script>

<Row>
	<Cell><b>{name}</b></Cell>
	<Cell style='padding-top: 10px;'>
		<Textfield input$rows="5" input$cols="60" style="width: 100%;" textarea bind:value={strings} label="Strings to enable against. Use new line for each new string" />
	</Cell>
	<Cell style='padding-top: 10px;'>
		<Textfield input$rows="2" input$cols="30" textarea bind:value={description} label="description" />
	</Cell>
	<Cell>
		<IconButton class="material-icons" {disabled} on:click={() => save()}>save</IconButton>
	</Cell>
</Row>
