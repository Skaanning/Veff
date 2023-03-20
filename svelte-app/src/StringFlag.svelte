<script>
	import _ from "lodash";
	import IconButton from "@smui/icon-button";
	import { Row, Cell } from "@smui/data-table";
	import Textfield from "@smui/textfield";
	import { createEventDispatcher } from 'svelte';
	import Button from '@smui/button';
	import HelperText from '@smui/textfield/helper-text';

	export let strings;
	export let name;
	export let description;
	export let id;
	export let type;

	let disabled = false;
	const dispatch = createEventDispatcher();

	async function save() {
		let update = { "Id": id, "Description": description, "Type": type, "Percent": 0, "Strings": strings }
		const options = {
			method: "POST",
			body: JSON.stringify(update),
			headers: {"Content-Type": "application/json"},
		};

		disabled = true;
		let res = await fetch("/veff_internal_api/update", options);
		if (res.ok) {
			dispatch("saved", {req: update, msg: `updated flag ${name} with values ${strings}`})
		} else {
			dispatch("error", {message: "something went bad" })
		}

		disabled = false;
	}

	function getTypeDescription() {
		console.log(type);
		var s = type.split(/(?=[A-Z])/);
		if (s.length == 4) {
			return s[1] + " " + s[2];
		}
		return s[1];
	}

</script>

<Row>
	<Cell><b>{name} ({getTypeDescription()})</b></Cell>
	<Cell style="padding:1rem;">
		<Textfield 
			textarea bind:value={strings} label="string values">
		</Textfield>
	</Cell>
	<Cell style="width:30%">
		<Textfield style="width: 100%;" input$resizable={false} bind:value={description} label="description" />
	</Cell>
	<Cell>
		<IconButton class="material-icons" {disabled} on:click={() => save()}>save</IconButton>
	</Cell>
</Row>
