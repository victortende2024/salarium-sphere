import { apiClient } from '../lib/api';

// --- Types ---
export interface ProfileBenefit {
  id: string;
  benefitName: string;
  amount: number;
  currency: number;
  isRecurring: boolean;
}

export interface Profile {
  id: string;
  name: string;
  description?: string;
  baseSalaryAmount: number;
  baseSalaryCurrency: number;
  isActive: boolean;
  defaultBenefits: ProfileBenefit[];
}

export interface CreateProfileRequest {
  name: string;
  description?: string;
  baseSalaryAmount: number;
  baseSalaryCurrency: number;
  defaultBenefits: Omit<ProfileBenefit, 'id'>[];
}

export interface UpdateProfileRequest extends CreateProfileRequest {
  id: string;
}

// --- Service ---
export const profileService = {
  // GET all profiles
  getProfiles: async (includeInactive = false): Promise<Profile[]> => {
    const response = await apiClient.get('/profiles', { params: { includeInactive } });
    return response.data;
  },

  // GET single profile by ID
  getProfile: async (id: string): Promise<Profile> => {
    const response = await apiClient.get(`/profiles/${id}`);
    return response.data;
  },

  // CREATE a new profile
  createProfile: async (data: CreateProfileRequest): Promise<Profile> => {
    const response = await apiClient.post('/profiles', data);
    return response.data;
  },

  // UPDATE an existing profile
  updateProfile: async (data: UpdateProfileRequest): Promise<Profile> => {
    const response = await apiClient.put(`/profiles/${data.id}`, data);
    return response.data;
  },

  // DELETE a profile (soft delete)
  deleteProfile: async (id: string): Promise<void> => {
    await apiClient.delete(`/profiles/${id}`);
  },
};