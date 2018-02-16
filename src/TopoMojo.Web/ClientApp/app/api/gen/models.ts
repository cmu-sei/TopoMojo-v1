export interface AccountsCredentials {
	username?: string;
	password?: string;
	code?: number;
}

export interface Message {
	id?: number;
	roomId?: string;
	authorName?: string;
	text?: string;
	whenCreated?: string;
	edited?: boolean;
}

export interface NewMessage {
	roomId?: string;
	text?: string;
}

export interface ChangedMessage {
	id?: number;
	text?: string;
}

export interface ImageFile {
	filename?: string;
}

export interface Gamespace {
	id?: number;
	globalId?: string;
	name?: string;
	whenCreated?: string;
	topologyDocument?: string;
	topologyId?: number;
	players?: Array<Player>;
}

export interface Player {
	id?: number;
	personId?: number;
	personName?: string;
	personGlobalId?: string;
	canManage?: boolean;
	canEdit?: boolean;
}

export interface GameState {
	id?: number;
	name?: string;
	globalId?: string;
	whenCreated?: string;
	topologyDocument?: string;
	shareCode?: string;
	vms?: Array<VmState>;
}

export interface VmState {
	id?: string;
	templateId?: number;
	name?: string;
	isRunning?: boolean;
}

export interface ProfileSearchResult {
	search?: Search;
	total?: number;
	results?: Array<Profile>;
}

export interface Search {
	term?: string;
	skip?: number;
	take?: number;
	sort?: number;
	filters?: Array<string>;
}

export interface Profile {
	id?: number;
	globalId?: string;
	name?: string;
	isAdmin?: boolean;
}

export interface ChangedProfile {
	globalId?: string;
	name?: string;
}

export interface TemplateSummarySearchResult {
	search?: Search;
	total?: number;
	results?: Array<TemplateSummary>;
}

export interface TemplateSummary {
	id?: number;
	name?: string;
	description?: string;
	topologyId?: number;
	topologyName?: string;
	parentId?: string;
	parentName?: string;
}

export interface Template {
	id?: number;
	parentId?: number;
	canEdit?: boolean;
	name?: string;
	description?: string;
	networks?: string;
	iso?: string;
	isHidden?: boolean;
	topologyId?: number;
	topologyGlobalId?: string;
}

export interface TemplateDetail {
	id?: number;
	name?: string;
	description?: string;
	networks?: string;
	detail?: string;
	isPublished?: boolean;
}

export interface ChangedTemplate {
	id?: number;
	name?: string;
	description?: string;
	networks?: string;
	iso?: string;
	isHidden?: boolean;
	topologyId?: number;
}

export interface TopologySummarySearchResult {
	search?: Search;
	total?: number;
	results?: Array<TopologySummary>;
}

export interface TopologySummary {
	id?: number;
	name?: string;
	description?: string;
	canManage?: boolean;
	canEdit?: boolean;
	isPublished?: boolean;
	isLocked?: boolean;
	author?: string;
}

export interface TopologySearchResult {
	search?: Search;
	total?: number;
	results?: Array<Topology>;
}

export interface Topology {
	id?: number;
	globalId?: string;
	name?: string;
	description?: string;
	document?: string;
	shareCode?: string;
	author?: string;
	whenCreated?: string;
	canManage?: boolean;
	canEdit?: boolean;
	templateLimit?: number;
	isPublished?: boolean;
	isLocked?: boolean;
	workers?: Array<Worker>;
	templates?: Array<Template>;
}

export interface Worker {
	id?: number;
	personName?: string;
	personGlobalId?: string;
	canManage?: boolean;
	canEdit?: boolean;
}

export interface NewTopology {
	name?: string;
	description?: string;
}

export interface ChangedTopology {
	id?: number;
	name?: string;
	description?: string;
	author?: string;
}

export interface TopologyState {
	id?: number;
	shareCode?: string;
	isPublished?: boolean;
	isLocked?: boolean;
}

export interface VmOptions {
	iso?: Array<string>;
	net?: Array<string>;
}

export interface VirtualVm {
	id?: string;
	name?: string;
	host?: string;
	path?: string;
	reference?: string;
	diskPath?: string;
	stats?: string;
	status?: string;
	groupName?: string;
	state?: VirtualVmStateEnum;
	question?: VirtualVmQuestion;
	task?: VirtualVmTask;
}

export interface VirtualVmQuestion {
	id?: string;
	prompt?: string;
	defaultChoice?: string;
	choices?: Array<VirtualVmQuestionChoice>;
}

export interface VirtualVmTask {
	id?: string;
	name?: string;
	progress?: number;
	whenCreated?: string;
}

export interface VirtualVmQuestionChoice {
	key?: string;
	label?: string;
}

export interface KeyValuePair {
	id?: number;
	key?: string;
	value?: string;
}

export interface VirtualVmAnswer {
	questionId?: string;
	choiceKey?: string;
}

export enum VirtualVmStateEnum {
	off = <any>'off',
	running = <any>'running',
	suspended = <any>'suspended'
}

