<script>
	import _ from "lodash";
	import IconButton from "@smui/icon-button";
	import { Row, Cell } from "@smui/data-table";
	import Textfield from "@smui/textfield";
	import FormField from "@smui/form-field";
	import { createEventDispatcher } from 'svelte';

	export let value;
	export let name;
	export let description;
	export let id;

	const dispatch = createEventDispatcher();

	let disabled = false;

	async function save() {
		let update = { "Id": id, "Description": description, "Value": value, "Type": "DateFlag" };
		const options = {
			method: "POST",
			body: JSON.stringify(update),
			headers: { "Content-Type": "application/json", },
		};

		disabled = true;
		let res = await fetch("/veff_internal_api/update", options);
		if (res.ok) {
			dispatch("saved", {req: update, msg: `updated flag ${name} to value ${value}`})
		} else {
			dispatch("error", {message: "something went bad" })
		}
		disabled = false;
	}
</script>

<Row>
	<Cell><b>{name}</b></Cell>
	<Cell style="padding:1rem;">
		<FormField>
			<input type="date" bind:value={value} />
			<span slot="label">Date value</span>
		</FormField>
	</Cell>
	<Cell>
		<Textfield
			input$resizable={false}
			style="width: 100%;"
			bind:value={description}
			label="description"
		/>
	</Cell>
	<Cell>
		<IconButton class="material-icons" {disabled} on:click={() => save()}>save</IconButton>
	</Cell>
</Row>

