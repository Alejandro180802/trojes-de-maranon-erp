import { Body, Controller, Post } from '@nestjs/common';
import { IsIn, IsString, MaxLength } from 'class-validator';
import { StorageService } from './storage.service';
class SignedUploadDto { @IsString() @MaxLength(180) filename!: string; @IsIn(['image/jpeg', 'image/png', 'image/webp']) contentType!: string; }
class LocalUploadDto extends SignedUploadDto { @IsString() @MaxLength(8_400_000) dataUrl!: string; }
@Controller('uploads') export class StorageController {
  constructor(private readonly storage: StorageService) {}
  @Post('signed-url') signedUrl(@Body() dto: SignedUploadDto) { return this.storage.createSignedUpload(dto.filename, dto.contentType); }
  @Post('local') local(@Body() dto: LocalUploadDto) { return this.storage.saveLocalEvidence(dto.filename, dto.contentType, dto.dataUrl); }
}
