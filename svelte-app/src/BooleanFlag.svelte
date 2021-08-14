<script>
	import _ from "lodash";
	import Switch from "@smui/switch";
	import IconButton from "@smui/icon-button";
	import { Row, Cell } from "@smui/data-table";
	import Textfield from "@smui/textfield";

	export let checked;
	export let name;
	export let description;
	export let id;

	let disabled = false;

	async function save() {
		const options = {
			method: "POST",
			body: JSON.stringify({ "id": id, "description": description, "type": "BooleanFlag", "percent": checked ? 100 : 0, "strings": "" }),
			headers: {	"Content-Type": "application/json",},
		};

		disabled = true;
		let res = await fetch("/veff_internal_api/update", options);
		disabled = false;
	}
</script>

<Row>
	<Cell><b>{name}</b></Cell>
	<Cell>
		<Switch bind:checked />
	</Cell>
	<Cell style="padding-top: 10px;">
		<Textfield
			input$rows="2"
			input$cols="30"
			textarea
			bind:value={description}
			label="description"
		/>
	</Cell>
	<Cell>
		<IconButton class="material-icons" {disabled} on:click={() => save()}>save</IconButton
		>
	</Cell>
</Row>
