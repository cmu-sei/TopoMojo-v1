export interface AccountsCredentials {
	username?: string;
	password?: string;
	code?: number;
}

export interface ImageFile {
	filename?: string;
}

export interface HttpIFormFile {
	contentType?: string;
	contentDisposition?: string;
	headers?: object;
	length?: number;
	name?: string;
	fileName?: string;
}

export interface Gamespace {
	id?: number;
	whenCreated?: string;
	document?: string;
	vmCount?: number;
}

export interface GameState {
	id?: number;
	globalId?: string;
	whenCreated?: string;
	document?: string;
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

export interface TemplateSummarySearchResult {
	search?: Search;
	total?: number;
	results?: Array<TemplateSummary>;
}

export interface TemplateSummary {
	id?: number;
	name?: string;
	topologyId?: number;
	topologyName?: string;
	parentId?: string;
	parentName?: string;
}

export interface TemplateDetailSearchResult {
	search?: Search;
	total?: number;
	results?: Array<TemplateDetail>;
}

export interface TemplateDetail {
	id?: number;
	name?: string;
	networks?: string;
	detail?: string;
	isPublished?: boolean;
}

export interface Template {
	id?: number;
	canEdit?: boolean;
	name?: string;
	description?: string;
	networks?: string;
	iso?: string;
	topologyGlobalId?: string;
	parent?: TemplateSummary;
}

export interface NewTemplateDetail {
	name?: string;
	networks?: string;
	detail?: string;
	isPublished?: boolean;
}

export interface TopologyTemplate {
	id?: number;
	name?: string;
	parentName?: string;
}

export interface ChangedTemplate {
	id?: number;
	name?: string;
	description?: string;
	networks?: string;
	iso?: string;
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
	documentUrl?: string;
	shareCode?: string;
	canManage?: boolean;
	canEdit?: boolean;
	isPublished?: boolean;
	workers?: Array<Worker>;
	templates?: Array<Template>;
}

export interface Worker {
	id?: number;
	personName?: string;
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
}

export interface TopologyState {
	id?: number;
	shareCode?: string;
	isPublished?: boolean;
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

