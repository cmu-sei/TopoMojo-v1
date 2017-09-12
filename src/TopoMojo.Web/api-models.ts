export interface Gamespace {
	id?: number; 		// (Int32);
	whenCreated?: string; 		// (String);
	document?: string; 		// (String);
	vmCount?: number; 		// (Int32);
}

export interface GameState {
	id?: number; 		// (Int32);
	globalId?: string; 		// (String);
	whenCreated?: string; 		// (String);
	document?: string; 		// (String);
	shareCode?: string; 		// (String);
	vms?: Array<VmState>; 		// (IEnumerable`1);
}

export interface VmState {
	id?: string; 		// (String);
	templateId?: number; 		// (Int32);
	name?: string; 		// (String);
	isRunning?: boolean; 		// (Boolean);
}

export interface Profile {
	id?: number; 		// (Int32);
	globalId?: string; 		// (String);
	name?: string; 		// (String);
	isAdmin?: boolean; 		// (Boolean);
}

export interface Template {
	id?: number; 		// (Int32);
	canEdit?: boolean; 		// (Boolean);
	name?: string; 		// (String);
	description?: string; 		// (String);
	networks?: string; 		// (String);
	iso?: string; 		// (String);
	topologyGlobalId?: string; 		// (String);
	parent?: TemplateSummary; 		// (TemplateSummary);
}

export interface ChangedTemplate {
	id?: number; 		// (Int32);
	name?: string; 		// (String);
	description?: string; 		// (String);
	networks?: string; 		// (String);
	iso?: string; 		// (String);
}

export interface NewTemplateDetail {
	name?: string; 		// (String);
	networks?: string; 		// (String);
	detail?: string; 		// (String);
	isPublished?: boolean; 		// (Boolean);
}

export interface TemplateDetail {
	id?: number; 		// (Int32);
	name?: string; 		// (String);
	networks?: string; 		// (String);
	detail?: string; 		// (String);
	isPublished?: boolean; 		// (Boolean);
}

export interface TemplateSummary {
	id?: number; 		// (Int32);
	name?: string; 		// (String);
	topologyId?: number; 		// (Int32);
	topologyName?: string; 		// (String);
	parentId?: string; 		// (String);
	parentName?: string; 		// (String);
}

export interface Topology {
	id?: number; 		// (Int32);
	globalId?: string; 		// (String);
	name?: string; 		// (String);
	description?: string; 		// (String);
	documentUrl?: string; 		// (String);
	shareCode?: string; 		// (String);
	canManage?: boolean; 		// (Boolean);
	canEdit?: boolean; 		// (Boolean);
	isPublished?: boolean; 		// (Boolean);
	workers?: Array<Worker>; 		// (Worker[]);
	templates?: Array<TopologyTemplate>; 		// (TopologyTemplate[]);
}

export interface NewTopology {
	name?: string; 		// (String);
	description?: string; 		// (String);
}

export interface ChangedTopology {
	id?: number; 		// (Int32);
	name?: string; 		// (String);
	description?: string; 		// (String);
}

export interface TopologyState {
	id?: number; 		// (Int32);
	shareCode?: string; 		// (String);
	isPublished?: boolean; 		// (Boolean);
}

export interface Worker {
	id?: number; 		// (Int32);
	personName?: string; 		// (String);
	canManage?: boolean; 		// (Boolean);
	canEdit?: boolean; 		// (Boolean);
}

export interface TopologyTemplate {
	id?: number; 		// (Int32);
	name?: string; 		// (String);
	parentName?: string; 		// (String);
}

export interface ExtensionsModelExtensions+<>c__DisplayClass0_0 {
}

export interface ConsoleLoadModel {
	name?: string; 		// (String);
	ticket?: string; 		// (String);
}

export interface ManageViewModelsAddPhoneNumberViewModel {
	phoneNumber?: string; 		// (String);
}

export interface ManageViewModelsConfigureTwoFactorViewModel {
	selectedProvider?: string; 		// (String);
	providers?: Array<MicrosoftAspNetCoreMvcRenderingSelectListItem>; 		// (ICollection`1);
}

export interface ManageViewModelsFactorViewModel {
	purpose?: string; 		// (String);
}

export interface ManageViewModelsIndexViewModel {
	hasPassword?: boolean; 		// (Boolean);
	logins?: Array<MicrosoftAspNetCoreIdentityUserLoginInfo>; 		// (IList`1);
	phoneNumber?: string; 		// (String);
	twoFactor?: boolean; 		// (Boolean);
	browserRemembered?: boolean; 		// (Boolean);
}

export interface ManageViewModelsManageLoginsViewModel {
	currentLogins?: Array<MicrosoftAspNetCoreIdentityUserLoginInfo>; 		// (IList`1);
	otherLogins?: Array<MicrosoftAspNetCoreHttpAuthenticationAuthenticationDescription>; 		// (IList`1);
}

export interface ManageViewModelsRemoveLoginViewModel {
	loginProvider?: string; 		// (String);
	providerKey?: string; 		// (String);
}

export interface ManageViewModelsSetPasswordViewModel {
	newPassword?: string; 		// (String);
	confirmPassword?: string; 		// (String);
}

export interface ManageViewModelsVerifyPhoneNumberViewModel {
	code?: string; 		// (String);
	phoneNumber?: string; 		// (String);
}

export interface AccountViewModelsChangePasswordViewModel {
	current?: string; 		// (String);
	password?: string; 		// (String);
	confirmPassword?: string; 		// (String);
}

export interface AccountViewModelsExternalLoginConfirmationViewModel {
	email?: string; 		// (String);
}

export interface AccountViewModelsForgotPasswordViewModel {
	email?: string; 		// (String);
}

export interface AccountViewModelsLoginViewModel {
	email?: string; 		// (String);
	password?: string; 		// (String);
	rememberMe?: boolean; 		// (Boolean);
}

export interface AccountViewModelsProfileUpdateModel {
	name?: string; 		// (String);
}

export interface AccountViewModelsResetPasswordViewModel {
	email?: string; 		// (String);
	password?: string; 		// (String);
	confirmPassword?: string; 		// (String);
	code?: string; 		// (String);
}

export interface AccountViewModelsSendCodeViewModel {
	selectedProvider?: string; 		// (String);
	providers?: Array<MicrosoftAspNetCoreMvcRenderingSelectListItem>; 		// (ICollection`1);
	returnUrl?: string; 		// (String);
	rememberMe?: boolean; 		// (Boolean);
}

export interface AccountViewModelsVerifyCodeViewModel {
	provider?: string; 		// (String);
	code?: string; 		// (String);
	returnUrl?: string; 		// (String);
	rememberBrowser?: boolean; 		// (Boolean);
	rememberMe?: boolean; 		// (Boolean);
}

export interface PlayerModel {
	id?: number; 		// (Int32);
	name?: string; 		// (String);
	online?: boolean; 		// (Boolean);
}

export interface Map {
	id?: string; 		// (String);
	name?: string; 		// (String);
	image?: string; 		// (String);
	resolution?: string; 		// (String);
	palette?: string; 		// (String);
	nodes?: Array<MapNode>; 		// (MapNode[]);
}

export interface MapNode {
	name?: string; 		// (String);
	loc?: string; 		// (String);
	i?: number; 		// (Int32);
	hot?: boolean; 		// (Boolean);
	link?: boolean; 		// (Boolean);
}

export interface PodConfiguration {
	url?: string; 		// (String);
	host?: string; 		// (String);
	user?: string; 		// (String);
	password?: string; 		// (String);
	poolPath?: string; 		// (String);
	uplink?: string; 		// (String);
	vmStore?: string; 		// (String);
	diskStore?: string; 		// (String);
	isoStore?: string; 		// (String);
	stockStore?: string; 		// (String);
	displayMethod?: string; 		// (String);
	displayUrl?: string; 		// (String);
	vlan?: VlanOptions; 		// (VlanOptions);
}

export interface TemplateOptions {
	cpu?: Array<string>; 		// (String[]);
	ram?: Array<string>; 		// (String[]);
	adapters?: Array<string>; 		// (String[]);
	guest?: Array<string>; 		// (String[]);
	iso?: Array<string>; 		// (String[]);
	source?: Array<string>; 		// (String[]);
	palette?: Array<string>; 		// (String[]);
}

export interface VmOptions {
	iso?: Array<string>; 		// (String[]);
	net?: Array<string>; 		// (String[]);
}

export interface VlanOptions {
	range?: string; 		// (String);
	reservations?: Array<Vlan>; 		// (Vlan[]);
}

export interface Vlan {
	id?: number; 		// (Int32);
	name?: string; 		// (String);
}

export interface TaskStatus {
	id?: string; 		// (String);
	progress?: number; 		// (Int32);
}

export interface KeyValuePair {
	id?: number; 		// (Int32);
	key?: string; 		// (String);
	value?: string; 		// (String);
}

export interface VirtualTemplate {
	id?: string; 		// (String);
	name?: string; 		// (String);
	topoId?: string; 		// (String);
	cpu?: string; 		// (String);
	guest?: string; 		// (String);
	source?: string; 		// (String);
	iso?: string; 		// (String);
	floppy?: string; 		// (String);
	guestSettings?: Array<KeyValuePair>; 		// (KeyValuePair[]);
	version?: string; 		// (String);
	isolationTag?: string; 		// (String);
	ram?: number; 		// (Int32);
	videoRam?: number; 		// (Int32);
	adapters?: number; 		// (Int32);
	delay?: number; 		// (Int32);
	eth?: Array<VirtualEth>; 		// (Eth[]);
	disks?: Array<VirtualDisk>; 		// (Disk[]);
}

export interface VirtualEth {
	id?: number; 		// (Int32);
	net?: string; 		// (String);
	type?: string; 		// (String);
	mac?: string; 		// (String);
	ip?: string; 		// (String);
	vlan?: number; 		// (Int32);
}

export interface VirtualDisk {
	id?: number; 		// (Int32);
	path?: string; 		// (String);
	source?: string; 		// (String);
	controller?: string; 		// (String);
	size?: number; 		// (Int32);
	status?: number; 		// (Int32);
}

export interface VirtualVm {
	id?: string; 		// (String);
	name?: string; 		// (String);
	host?: string; 		// (String);
	path?: string; 		// (String);
	reference?: string; 		// (String);
	diskPath?: string; 		// (String);
	stats?: string; 		// (String);
	status?: string; 		// (String);
	state?: VirtualVmPowerState; 		// (VmPowerState);
	question?: VirtualVmQuestion; 		// (VmQuestion);
	task?: VirtualVmTask; 		// (VmTask);
}

export enum VirtualVmPowerState {
	off = <any>'off', 		// 0
	running = <any>'running', 		// 1
	suspended = <any>'suspended' 		// 2
}

export interface VirtualVmQuestion {
	id?: string; 		// (String);
	prompt?: string; 		// (String);
	defaultChoice?: string; 		// (String);
	choices?: Array<VirtualVmQuestionChoice>; 		// (VmQuestionChoice[]);
}

export interface VirtualVmAnswer {
	questionId?: string; 		// (String);
	choiceKey?: string; 		// (String);
}

export interface VirtualVmQuestionChoice {
	key?: string; 		// (String);
	label?: string; 		// (String);
}

export interface VirtualVmTask {
	id?: string; 		// (String);
	name?: string; 		// (String);
	progress?: number; 		// (Int32);
	whenCreated?: number; 		// (DateTime);
}

export interface VirtualDisplayInfo {
	id?: string; 		// (String);
	topoId?: string; 		// (String);
	name?: string; 		// (String);
	method?: string; 		// (String);
	url?: string; 		// (String);
	conditions?: string; 		// (String);
}

