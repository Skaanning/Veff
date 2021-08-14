<script>
	import _ from "lodash";
	import IconButton from "@smui/icon-button";
	import { Row, Cell } from "@smui/data-table";
	import Textfield from "@smui/textfield";

	export let strings;
	export let name;
	export let description;
	export let id;

	let disabled = false;

	async function save() {
		const options = {
			method: "POST",
			body: JSON.stringify({ "id": id, "description": description, "type": "StringFlag", "percent": 0, "strings": strings }),
			headers: {	"Content-Type": "application/json",},
		};

		disabled = true;
		let res = await fetch("/veff_internal_api/update", options);
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
