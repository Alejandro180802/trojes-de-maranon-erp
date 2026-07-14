import { Body, Controller, Get, Post, Req, UseGuards } from '@nestjs/common';
import { IsEmail, IsString, MinLength } from 'class-validator';
import { AuthService } from './auth.service';
import { JwtAuthGuard } from './jwt-auth.guard';
import { Public } from './public.decorator';
class LoginDto { @IsEmail() email!: string; @IsString() @MinLength(8) password!: string; }
class RefreshDto { @IsString() refreshToken!: string; }
@Controller('auth') export class AuthController {
  constructor(private readonly auth: AuthService) {}
  @Public() @Post('login') login(@Body() body: LoginDto) { return this.auth.login(body.email, body.password); }
  @Public() @Post('refresh') refresh(@Body() body: RefreshDto) { return this.auth.refresh(body.refreshToken); }
  @Public() @Post('logout') logout(@Body() body: RefreshDto) { return this.auth.logout(body.refreshToken).then(() => ({ success: true })); }
  @UseGuards(JwtAuthGuard) @Get('me') me(@Req() req: { user: unknown }) { return req.user; }
}
