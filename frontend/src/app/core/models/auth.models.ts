// Oblik podataka koji ulazi/izlazi kroz /api/auth (odgovara backend Dtos/Auth).

export interface UserSummary {
  id: string;
  email: string;
  displayName: string;
  username: string;
  avatarVersion: number | null;
}

export interface UsernameAvailability {
  available: boolean;
}

export interface AuthResponse {
  accessToken: string;
  accessTokenExpiresAt: string;
  user: UserSummary;
}

export interface MessageResponse {
  message: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  username: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface VerifyEmailRequest {
  email: string;
  code: string;
}

export interface ResendVerificationRequest {
  email: string;
}

export interface ForgotPasswordRequest {
  email: string;
}

export interface ResetPasswordRequest {
  email: string;
  code: string;
  newPassword: string;
}
