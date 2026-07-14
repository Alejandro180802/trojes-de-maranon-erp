import { BadRequestException, Injectable } from '@nestjs/common';
import { createClient } from '@supabase/supabase-js';
import { randomUUID } from 'crypto';
import { mkdir, writeFile } from 'fs/promises';
import { join } from 'path';
@Injectable() export class StorageService {
  async createSignedUpload(filename: string, contentType: string) {
    const url = process.env.SUPABASE_URL; const key = process.env.SUPABASE_SERVICE_ROLE_KEY;
    if (!url || !key) throw new BadRequestException('El almacenamiento de evidencias no está configurado');
    const path = `evidence/${new Date().toISOString().slice(0, 10)}/${randomUUID()}-${filename.replace(/[^a-zA-Z0-9._-]/g, '_')}`;
    const { data, error } = await createClient(url, key).storage.from('evidence').createSignedUploadUrl(path, { upsert: false });
    if (error || !data) throw new BadRequestException(error?.message ?? 'No fue posible preparar la carga');
    return { path, token: data.token, signedUrl: data.signedUrl, contentType };
  }
  async saveLocalEvidence(filename: string, contentType: string, dataUrl: string) {
    const base64 = dataUrl.split(',')[1];
    if (!base64) throw new BadRequestException('La evidencia no contiene datos válidos');
    const bytes = Buffer.from(base64, 'base64');
    if (bytes.length > 6 * 1024 * 1024) throw new BadRequestException('La evidencia supera el límite local de 6 MB');
    const safeName = `${randomUUID()}-${filename.replace(/[^a-zA-Z0-9._-]/g, '_')}`;
    const directory = join(process.cwd(), 'uploads', 'evidence');
    await mkdir(directory, { recursive: true });
    await writeFile(join(directory, safeName), bytes);
    return { path: `/uploads/evidence/${safeName}`, contentType };
  }
}
