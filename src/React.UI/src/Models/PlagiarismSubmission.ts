import { PlagiarismFile } from './PlagiarismFile';

/**
 * Plagiarism Set Submission Entity
 */
export interface PlagiarismSubmission {

  /**
   * Plagiarism Set Id
   */
  setid: string;

  /**
   * Submission Id
   */
  submitid: number;

  /**
   * External Submission Id
   */
  externalid: string;

  /**
   * Exclusive category Id
   */
  exclusive_category: number;

  /**
   * Non-exclusive category Id
   */
  inclusive_category: number;

  /**
   * Submission name
   */
  name: string;

  /**
   * Max percentage of detected plagiarism
   */
  max_percent?: number;

  /**
   * Whether the token is produced
   */
  token_produced: boolean | null;

  /**
   * Upload time (ISO format)
   */
  upload_time: string;

  /**
   * The file of submission
   */
  files: PlagiarismFile[] | null | undefined;

  /**
   * Language name
   */
  language: string;
}
