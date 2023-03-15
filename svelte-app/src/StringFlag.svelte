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

	let disabled = false;
	const dispatch = createEventDispatcher();

	async function save() {
		let update = { "Id": id, "Description": description, "Type": "StringFlag", "Percent": 0, "Strings": strings }
		const options = {
			method: "POST",
			body: JSON.stringify(update),
			headers: {"Content-Type": "application/json"},
		};

		disabled = true;
		let res = await fetch("/veff_internal_api/update", options);
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
	<Cell style="padding:1rem;">
		<Textfield 
			style="width: 100%;height:5rem"
			helperLine$style="width: 100%;"
				input$cols="80" 
			input$resizable={false}
			textarea bind:value={strings} label="Values">
				<HelperText slot="helper">Strings to enable against. Use new line for each new value</HelperText>
		</Textfield>
	</Cell>
	<Cell style="width:30%">
		<Textfield style="width: 100%;" input$resizable={false} bind:value={description} label="description" />
	</Cell>
	<Cell>
		<IconButton class="material-icons" {disabled} on:click={() => save()}>save</IconButton>
	</Cell>
</Row>
