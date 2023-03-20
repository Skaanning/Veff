<script>
	import _ from "lodash";
	import Slider from "@smui/slider";
	import IconButton from "@smui/icon-button";
	import { Row, Cell } from "@smui/data-table";
	import Textfield from "@smui/textfield";
	import FormField from "@smui/form-field";
	import Tooltip, { Wrapper } from "@smui/tooltip";
	import { createEventDispatcher } from "svelte";
	import { v4 as uuidv4 } from "uuid";

	export let percentage;
	export let name;
	export let description;
	export let id;
	export let strings;

	const dispatch = createEventDispatcher();

	let disabled = false;

	function rollNewRandomSeed() {
		strings = uuidv4();
		dispatch("updaterandom", {
			message:
				"This means that the same UUIDs will not be allowed for the same percentage",
		});
	}

	async function save() {
		let update = {
			Id: id,
			Description: description,
			Enabled: false,
			Type: "PercentageFlag",
			Percent: percentage,
			Strings: strings,
		};
		const options = {
			method: "POST",
			body: JSON.stringify(update),
			headers: { "Content-Type": "application/json" },
		};

		disabled = true;
		let res = await fetch("/veff_internal_api/update", options);
		if (res.ok) {
			dispatch("saved", {req: update, msg: `updated flag ${name} with value ${percentage}% and randomSeed ${strings}`});
		} else {
			dispatch("error", { message: "something went bad" });
		}

		disabled = false;
	}
</script>

<Row>
	<Cell><b>{name}</b></Cell>
	<Cell style="padding:1rem;">
		<FormField style="display: flex;">
			<Textfield
				bind:value={percentage}
				label="percentage"
				type="number"
			/>
			<div style="display: flex; align-items: center;">
				<Wrapper>
					<IconButton
						class="material-icons"
						on:click={() => rollNewRandomSeed()}>loop</IconButton>
					<Tooltip>Use new random seed</Tooltip>
				</Wrapper>
			</div>
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
		<IconButton class="material-icons" {disabled} on:click={() => save()}>
			save
		</IconButton>
	</Cell>
</Row>
