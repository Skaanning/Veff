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
	let value1;
	let value2;
	let addedValue;

	// derive value from value1 and value2
	$: addedValue = (value1 || value2)
			? `${value1 || ''};${value2 || ''}`
			: ";";
	
	$: [value1, value2] = (value && value.includes(';'))
			? value.split(';').map(v => toInputDate(v))
			: [value, null];

	function toInputDate(d) {
		if (!d) return '';
		let date = new Date(d);
		if (isNaN(date)) return '';
		const year = date.getFullYear();
		const month = String(date.getMonth() + 1).padStart(2, '0'); // Months are 0-indexed
		const day = String(date.getDate()).padStart(2, '0');

		return `${year}-${month}-${day}`;
	}
		// if (!d && d !== 0) return '';
		// if (d instanceof Date) return d.toISOString().slice(0, 10);
		// if (typeof d === 'number') return new Date(d).toISOString().slice(0, 10);
		// if (typeof d === 'string') {
		// 	// try parsing an ISO string or accept already "YYYY-MM-DD"
		// 	const parsed = new Date(d);
		// 	if (!isNaN(parsed)) return parsed.toISOString().slice(0, 10);
		// 	// fallback: assume it's already "YYYY-MM-DD"
		// 	return d;
		// }
		// return '';
	// }
	
	const dispatch = createEventDispatcher();

	let disabled = false;

	async function save() {
		let update = { "Id": id, "Description": description, "Strings": addedValue, "Type": "DateFlag" };
		const options = {
			method: "POST",
			body: JSON.stringify(update),
			headers: { "Content-Type": "application/json", },
		};

		disabled = true;
		let res = await fetch("/veff_internal_api/update", options);
		if (res.ok) {
			dispatch("saved", {req: update, msg: `updated flag ${name} to value ${addedValue}`})
		} else {
			dispatch("error", {message: "something went bad" })
		}
		disabled = false;
	}
</script>

<Row>
	<Cell><b>{name}</b></Cell>
	<Cell style="padding:1rem;">
		<FormField align="end">
			<span slot="label">From date:</span>
			<input type="date" bind:value={value1} />
		</FormField>
		<br/>
		<FormField align="end">
			<span slot="label" style="white-space: pre">To date:    </span>
			<input type="date" bind:value={value2} />
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

