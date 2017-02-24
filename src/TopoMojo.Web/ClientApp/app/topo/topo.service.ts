import { AuthHttp } from '../core/auth-http';
import { Injectable } from '@angular/core';

@Injectable()
export class TopoService {

    constructor(
        private http: AuthHttp
        ) {}

    // public loadTopo(id: number) {
    //     return this.http.get('/api/topology/load/'+id);
    // }

    public createTopo(Topo) {
        return this.http.post('/api/topology/create', Topo);
    }

    public loadTopo(id) {
        return this.http.get('/api/topology/load/' + id);
    }

    public listtopo(search) {
        return this.http.post('/api/topology/list', search);
    }

    public updateTopo(topo) {
        return this.http.post('/api/topology/update', topo);
    }

    public deleteTopo(topo) {
        return this.http.delete('/api/topology/delete/' + topo.id);
    }

    public listMembers(id: number) {
        return this.http.get('/api/topology/members/' + id);
    }

    public addMembers(id: number, emails: string) {
        return this.http.post('/api/account/addtopouser', { topoId: id, emails: emails });
    }

    public removeMember(id: number, personId: number) {
        return this.http.post('/api/account/removetopouser', { topoId: id, personId: personId});
    }

    public createTemplate(template) {
        return this.http.post('/api/template/create', template);
    }

    public listTopoTemplates(id) {
        return this.http.get("/api/topology/templates/"+id);
    }

    public listTemplates(search) {
        return this.http.post('/api/template/list', search);
    }

    public loadTemplate(id) {
        return this.http.get('/api/template/load/'+id);
    }

    public saveTemplate(template) {
        return this.http.post('/api/template/save', template);
    }

    public deleteTemplate(id) {
        return this.http.delete('/api/template/delete/' + id);
    }

    public addTemplate(tref) {
        return this.http.post('/api/topology/addtemplate', tref);
    }

    public updateTemplate(tref) {
        return this.http.post('/api/topology/updatetemplate', tref);
    }

    public removeTemplate(tref) {
        return this.http.delete('/api/template/remove/'+tref.id);
    }

    public cloneTemplate(tref) {
        return this.http.post('/api/topology/clonetemplate/'+tref.id, {});
    }

    public onError(err) {
        this.http.onError(err);
    }
}