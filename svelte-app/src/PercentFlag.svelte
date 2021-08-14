<script>
	import _ from "lodash";
	import IconButton from "@smui/icon-button";
	import { Row, Cell } from "@smui/data-table";
	import Textfield from "@smui/textfield";
	import Slider from "@smui/slider";
	import FormField from "@smui/form-field";

	export let percent;
	export let name;
	export let description;
	export let id;

	let disabled = false;
	$: description;

	async function save() {
		const options = {
			method: "POST",
			body: JSON.stringify({ "id": id, "description": description, "type": "PercentFlag", "percent": percent, "strings": "" }),
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
		<FormField align="end" style="display: flex;">
			<Slider style="flex-grow: 1;" bind:value={percent} />
			<span slot="label"><b>{percent}%</b></span>
		</FormField>
	</Cell>
	<Cell style='padding-top: 10px;'>
		<Textfield input$rows="2" input$cols="30" textarea bind:value={description} label="description" />
	</Cell>
	<Cell>
		<IconButton class="material-icons" {disabled} on:click={() => save()}>save</IconButton
		>
	</Cell>
</Row>
