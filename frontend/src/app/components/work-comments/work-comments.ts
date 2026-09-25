import { DatePipe, NgTemplateOutlet } from '@angular/common';
import { Component, OnInit, inject, input, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslocoPipe, TranslocoService } from '@jsverse/transloco';
import { ButtonModule } from 'primeng/button';
import { DialogService } from 'primeng/dynamicdialog';
import { EditorModule } from 'primeng/editor';
import { Avatar } from '../avatar/avatar';
import { ConfirmDialog, ConfirmDialogData } from '../confirm-dialog/confirm-dialog';
import { WorkComment } from '../../core/models/comment.models';
import { CommentStore } from '../../core/services/comment-store';
import { extractErrorMessage } from '../../core/utils/http-error';

function hasContent(html: string | null): boolean {
  return (html ?? '').replace(/<[^>]*>/g, '').trim().length > 0;
}

@Component({
  selector: 'app-work-comments',
  imports: [FormsModule, EditorModule, ButtonModule, DatePipe, NgTemplateOutlet, TranslocoPipe, Avatar],
  templateUrl: './work-comments.html',
  styleUrl: './work-comments.scss',
})
export class WorkComments implements OnInit {
  readonly workId = input.required<string>();
  readonly canWrite = input(true);

  private readonly dialogService = inject(DialogService);
  private readonly transloco = inject(TranslocoService);

  protected readonly store = inject(CommentStore);

  protected readonly newText = signal('');
  protected readonly editingId = signal<string | null>(null);
  protected readonly editText = signal('');
  protected readonly saving = signal(false);
  protected readonly error = signal<string | null>(null);

  ngOnInit(): void {
    this.store.reset();
    this.store.load(this.workId());
  }

  protected canSubmit(html: string): boolean {
    return hasContent(html);
  }

  protected add(): void {
    if (!hasContent(this.newText())) {
      return;
    }

    this.saving.set(true);
    this.error.set(null);

    this.store.create(this.newText()).subscribe({
      next: () => {
        this.newText.set('');
        this.saving.set(false);
      },
      error: (err) => this.fail(err),
    });
  }

  protected startEdit(comment: WorkComment): void {
    this.error.set(null);
    this.editText.set(comment.text);
    this.editingId.set(comment.id);
  }

  protected cancelEdit(): void {
    this.editingId.set(null);
  }

  protected saveEdit(commentId: string): void {
    if (!hasContent(this.editText())) {
      return;
    }

    this.saving.set(true);
    this.error.set(null);

    this.store.update(commentId, this.editText()).subscribe({
      next: () => {
        this.editingId.set(null);
        this.saving.set(false);
      },
      error: (err) => this.fail(err),
    });
  }

  protected delete(comment: WorkComment): void {
    const title = this.transloco.translate('comments.deleteTitle');
    const data: ConfirmDialogData = {
      title,
      message: this.transloco.translate('common.cannotBeUndone'),
      confirmLabel: this.transloco.translate('common.delete'),
      destructive: true,
    };

    const ref = this.dialogService.open<ConfirmDialog, ConfirmDialogData>(ConfirmDialog, {
      header: title,
      width: '420px',
      modal: true,
      data,
    });

    ref?.onClose.subscribe((confirmed) => {
      if (!confirmed) {
        return;
      }
      this.store.remove(comment.id).subscribe({ error: (err) => this.error.set(extractErrorMessage(err)) });
    });
  }

  private fail(err: unknown): void {
    this.error.set(extractErrorMessage(err));
    this.saving.set(false);
  }
}
