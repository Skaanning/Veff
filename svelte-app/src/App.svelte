<script>
	import { onMount } from "svelte";
	import Tab, { Label } from "@smui/tab";
	import TabBar from "@smui/tab-bar";
	import Paper, { Title, Content } from "@smui/paper";
	import BooleanFlag from "./BooleanFlag.svelte";
	import StringFlag from "./StringFlag.svelte";
	import PercentageFlag from "./PercentageFlag.svelte";
	import _ from "lodash";
	import DataTable, { Head, Body, Row, Cell } from "@smui/data-table";
	import Snackbar from "@smui/snackbar";

	let flags = [];
	let flagsForSelectedTab = [];
	let tabs = [];
	let active = "";
	let savedSnackbar;
	let errorSnackbar;
	let randomSeedSnackbar;
	let savedMsg = "";

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
		errorSnackbar.open();
	}

	function handleUpdateRandom(err) {
		randomSeedSnackbar.open();
	}

	function handleSaved(savedEvent) {
		savedSnackbar.open();
		let update = savedEvent.detail.req;
		savedMsg = savedEvent.detail.msg ?? "";
		let index = _.findIndex(flags, (x) => x.Id === update.Id);

		let hll = flags[index];

		let updatedModel = { ...hll, ...update };

		flags[index] = updatedModel;
	}
</script>

{#if active}
	<div>
		<TabBar {tabs} let:tab bind:active on:click={() => updateActive()}>
			<Tab {tab}>
				<Label>{tab}</Label>
			</Tab>
		</TabBar>

		<Paper style="background-color:rgb(157 113 113 / 10%);">
			<Title style="width:80%;margin-left: auto; margin-right:auto;">{active}</Title>

			<Content style="width:80%;margin-left: auto; margin-right:auto;">
				<DataTable style="width:100%;padding:2rem;">
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
							{:else if f.Type == "PercentageFlag"}
								<PercentageFlag
									name={f.Name}
									id={f.Id}
									percentage={f.Percent}
									description={f.Description}
									strings={f.strings}
									on:error={handleError}
									on:saved={handleSaved}
									on:updaterandom={handleUpdateRandom}
								/>
							{:else}
								<StringFlag
									name={f.Name}
									id={f.Id}
									strings={f.Strings}
									description={f.Description}
									type={f.Type}
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
			<Label>{savedMsg}</Label>
		</Snackbar>

		<Snackbar bind:this={errorSnackbar}>
			<Label>Error!</Label>
		</Snackbar>

		<Snackbar bind:this={randomSeedSnackbar}>
			<Label>Updated random seed, not yet saved</Label>
		</Snackbar>
	</div>
{:else}
	<h1>Something is very, very wrong..</h1>
{/if}
