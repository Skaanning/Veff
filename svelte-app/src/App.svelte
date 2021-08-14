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

	let flags = [];
	let flagsForSelectedTab = [];
	let tabs;
	let tab = undefined;
	let active = undefined;

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
								/>
							{:else if f.Type == "StringFlag"}
								<StringFlag
									name={f.Name}
									id={f.Id}
									strings={_.join(f.Strings, "\n")}
									description={f.Description}
								/>
							{:else if f.Type == "PercentFlag"}
								<PercentFlag
									name={f.Name}
									id={f.Id}
									percent={f.Percent}
									description={f.Description}
								/>
							{/if}
						{/each}
					</Body>
				</DataTable>
			</Content>
		</Paper>
	</div>
{:else}
	<h1>hello</h1>
{/if}
