import { AuthHttp } from "../auth/auth-http";
import { Injectable } from "@angular/core";
import { SettingsService } from "../auth/settings.service";

@Injectable()
export class TopoService {

    constructor(
        private http: AuthHttp,
        private settings: SettingsService
        ) {}

    // public loadTopo(id: number) {
    //     return this.http.get(this.url() + "/topology/load/"+id);
    // }

    private url() {
        return this.settings.urls.apiUrl;
    }

    public ipCheck() {
        return this.http.get(this.url() + "/topology/ipcheck");
    }

    public createTopo(topo) {
        return this.http.post(this.url() + "/topology/create", topo);
    }

    public loadTopo(id) {
        return this.http.get(this.url() + "/topology/load/" + id);
    }

    public listtopo(search) {
        return this.http.post(this.url() + "/topology/list", search);
    }

    public listmine(search) {
        return this.http.post(this.url() + "/topology/mine", search);
    }

    public updateTopo(topo) {
        return this.http.post(this.url() + "/topology/update", topo);
    }

    public deleteTopo(topo) {
        return this.http.delete(this.url() + "/topology/delete/" + topo.id);
    }

    public listMembers(id: number) {
        return this.http.get(this.url() + "/topology/members/" + id);
    }

    public addMembers(id: number, emails: string) {
        return this.http.post(this.url() + "/account/addtopouser", { topoId: id, emails: emails });
    }

    public enlist(code: string) {
        return this.http.get(this.url() + "/topology/enlist/" + code);
    }

    public delist(id: number, mid: number) {
        return this.http.delete(this.url() + "/topology/delist/" + id + "/" + mid);
    }

    public share(id: number) {
        return this.http.get(this.url() + "/topology/share/" + id);
    }

    public unshare(id: number) {
        return this.http.get(this.url() + "/topology/unshare/" + id);
    }

    public publish(id: number) {
        return this.http.put(this.url() + "/topology/publish/" + id, null);
    }

    public unpublish(id: number) {
        return this.http.put(this.url() + "/topology/unpublish/" + id, null);
    }

    public removeMember(id: number, personId: number) {
        return this.http.post(this.url() + "/account/removetopouser", { topoId: id, personId: personId});
    }

    public createTemplate(template) {
        return this.http.post(this.url() + "/template/create", template);
    }

    public listTopoTemplates(id) {
        return this.http.get(this.url() + "/topology/templates/"+id);
    }

    public listTemplates(search) {
        return this.http.post(this.url() + "/template/list", search);
    }

    public loadTemplate(id) {
        return this.http.get(this.url() + "/template/load/"+id);
    }

    public saveTemplate(template) {
        return this.http.post(this.url() + "/template/save", template);
    }

    public deleteTemplate(id) {
        return this.http.delete(this.url() + "/template/delete/" + id);
    }

    public addTemplate(tref) {
        return this.http.post(this.url() + "/topology/addtemplate", tref);
    }

    public updateTemplate(tref) {
        return this.http.post(this.url() + "/topology/updatetemplate", tref);
    }

    public removeTemplate(tref) {
        return this.http.delete(this.url() + "/template/remove/"+tref.id);
    }

    public cloneTemplate(tref) {
        return this.http.post(this.url() + "/topology/clonetemplate/"+tref.id, {});
    }

    public launchInstance(id) {
        return this.http.get(this.url() + "/instance/launch/" + id);
    }

    public checkInstance(id) {
        return this.http.get(this.url() + "/instance/check/" + id);
    }

    public destroyInstance(id) {
        return this.http.delete(this.url() + "/instance/destroy/" + id);
    }

    public activeInstances() {
        return this.http.get(this.url() + "/instance/active");
    }

    public saveDoc(id, text) {
        return this.http.post(this.url() + "/topology/savedocument/" + id, text);
    }

    public loadDoc(id) {
        return this.http.gettext("/docs/" + id + ".md");
    }

    public loadUrl(url) {
        return this.http.gettext(url);
    }

    public getIsos(id) {
        return this.http.get(this.url() + "/topology/isos/" + id);
    }

    public uploadIso(id : string, file : File) {
        let payload : FormData = new FormData();
        let filename = `fn=${file.name}&fs=${file.size}&fk=${id}&fd=private`;
        payload.append('file', file, filename);
        return this.http.post(this.url() + '/file/upload', payload);
    }

    public onError(err) {
        this.http.onError(err);
    }
}