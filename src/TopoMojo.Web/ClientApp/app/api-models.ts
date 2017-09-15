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

export interface PlayerModel {
	id?: number;
	name?: string;
	online?: boolean;
}

export interface Profile {
	id?: number;
	globalId?: string;
	name?: string;
	isAdmin?: boolean;
}

export interface Search {
	term?: string;
	skip?: number;
	take?: number;
	sort?: number;
	filters?: Array<string>;
}

export interface SearchFilter {
	name?: string;
	id?: number;
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

export interface ChangedTemplate {
	id?: number;
	name?: string;
	description?: string;
	networks?: string;
	iso?: string;
}

export interface NewTemplateDetail {
	name?: string;
	networks?: string;
	detail?: string;
	isPublished?: boolean;
}

export interface TemplateDetail {
	id?: number;
	name?: string;
	networks?: string;
	detail?: string;
	isPublished?: boolean;
}

export interface TemplateSummary {
	id?: number;
	name?: string;
	topologyId?: number;
	topologyName?: string;
	parentId?: string;
	parentName?: string;
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
	templates?: Array<TopologyTemplate>;
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

export interface Worker {
	id?: number;
	personName?: string;
	canManage?: boolean;
	canEdit?: boolean;
}

export interface TopologyTemplate {
	id?: number;
	name?: string;
	parentName?: string;
}

