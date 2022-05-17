<script>
	import _ from "lodash";
	import Switch from "@smui/switch";
	import IconButton from "@smui/icon-button";
	import { Row, Cell } from "@smui/data-table";
	import Textfield from "@smui/textfield";
	import FormField from "@smui/form-field";
	import Checkbox from "@smui/checkbox";
	import { createEventDispatcher } from 'svelte';

	export let checked;
	export let name;
	export let description;
	export let id;

	const dispatch = createEventDispatcher();

	let disabled = false;

	async function save() {
		let update = { "Id": id, "Description": description, "Enabled": checked, "Type": "BooleanFlag", "Percent": checked ? 100 : 0, "Strings": "" };
		const options = {
			method: "POST",
			body: JSON.stringify(update),
			headers: {	"Content-Type": "application/json",},
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
		<FormField>
			<Checkbox bind:checked={checked} />
			<span slot="label">Is enabled</span>
		</FormField>
	</Cell>
	<Cell>
		<Textfield
			input$resizable={false}
			bind:value={description}
			label="description"
		/>
	</Cell>
	<Cell>
		<IconButton class="material-icons" {disabled} on:click={() => save()}>save</IconButton>
	</Cell>
</Row>

