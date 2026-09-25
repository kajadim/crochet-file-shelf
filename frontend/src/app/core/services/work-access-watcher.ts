import { Injectable, inject } from '@angular/core';
import { Router } from '@angular/router';
import { TranslocoService } from '@jsverse/transloco';
import { MessageService } from 'primeng/api';
import { Subscription, filter } from 'rxjs';
import { WorkRole } from '../models/work.models';
import { Realtime } from './realtime';

@Injectable({
  providedIn: 'root',
})
export class WorkAccessWatcher {
  private readonly realtime = inject(Realtime);
  private readonly router = inject(Router);
  private readonly messages = inject(MessageService);
  private readonly transloco = inject(TranslocoService);

  watch(workId: string, onRole: (role: WorkRole) => void): Subscription {
    return this.realtime.accessChanged$.pipe(filter((event) => event.workId === workId)).subscribe((event) => {
      if (event.role === null) {
        this.messages.add({ severity: 'warn', summary: this.transloco.translate('sharing.accessLost') });
        this.router.navigate(['/']);
        return;
      }
      onRole(event.role);
    });
  }
}
