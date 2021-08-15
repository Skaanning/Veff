<script>
	import { onMount } from "svelte";
	import Tab, { Label } from "@smui/tab";
	import TabBar from "@smui/tab-bar";
	import Paper, { Title, Content } from "@smui/paper";
	import BooleanFlag from "./BooleanFlag.svelte";
	import StringFlag from "./StringFlag.svelte";
	import PercentFlag from "./PercentFlag.svelte";
	import _ from "lodash";
	import DataTable, { Head, Body, Row, Cell } from "@smui/data-table";
	import Snackbar from '@smui/snackbar';

	let flags = [];
	let flagsForSelectedTab = [];
	let tabs;
	let active = undefined;
	let savedSnackbar;
	let errorSnackbar;

	onMount(async () => {
		const res = await fetch(`/veff_internal_api/init`);
		flags = (await res.json()).Flags;
		let stabs = _.map(flags, (x) => x.ContainerName);
		tabs = _.uniq(stabs);
		active = tabs[0];
		flagsForSelectedTab = _.filter(flags, (x) => x.ContainerName == active);
	});

	function updateActive() {
		flagsForSelectedTab = _.filter(flags, (x) => x.ContainerName == active);
	}

	function handleError(err) {
		errorSnackbar.open()
	}
	
	function handleSaved(savedEvent) {
		savedSnackbar.open()
		let update = savedEvent.detail
		console.table(update);

		let index = _.findIndex(flags, (x) => x.Id === update.Id)

		let hll = flags[index]
		console.log(update, hll)

		let updatedModel =  {...hll, ...update};
		console.log(updatedModel)

		flags[index] = updatedModel
	}
</script>

{#if tabs}
	<div>
		<!-- Note: tabs must be unique. (They cannot === each other.)-->
		<TabBar {tabs} let:tab bind:active on:click={() => updateActive()}>
			<!-- Note: the `tab` property is required! -->
			<Tab {tab} minWidth>
				<Label>{tab}</Label>
			</Tab>
		</TabBar>
		<Paper elevation="10">
			<Content>
				<DataTable style="width: 70%;">
					<Head>
						<Row>
							<Cell style="width: 10%;"><b>Feature Flag</b></Cell>
							<Cell>Value</Cell>
							<Cell>Description</Cell>
							<Cell style="width: 5%;" />
						</Row>
					</Head>
					<Body>
						{#each flagsForSelectedTab as f}
							{#if f.Type == "BooleanFlag"}
								<BooleanFlag
									name={f.Name}
									id={f.Id}
									checked={f.Enabled}
									description={f.Description}
									on:error={handleError}
									on:saved={handleSaved}
								/>
							{:else if f.Type == "StringFlag"}
								<StringFlag
									name={f.Name}
									id={f.Id}
									strings={f.Strings}
									description={f.Description}
									on:error={handleError}
									on:saved={handleSaved}
								/>
							{:else if f.Type == "PercentFlag"}
								<PercentFlag
									name={f.Name}
									id={f.Id}
									percent={f.Percent}
									description={f.Description}
									on:error={handleError}
									on:saved={handleSaved}
								/>
							{/if}
						{/each}
					</Body>
				</DataTable>
			</Content>
		</Paper>

		<Snackbar bind:this={savedSnackbar}>
			<Label>Saved!</Label>
		</Snackbar>

		<Snackbar bind:this={errorSnackbar}>
			<Label>Error!</Label>
		</Snackbar>
	</div>
{:else}
	<h1>Something is very, very wrong..</h1>
{/if}
