/**
 * Plagiarism Set Entity
 */
export interface PlagiarismSet {

  /**
   * Plagiarism Set Id
   */
  setid: string;

  /**
   * Create time
   */
  create_time: string;

  /**
   * Creator user Id
   */
  creator: number | null | undefined;

  /**
   * Contest Id
   */
  related: number | null | undefined;

  /**
   * Formal name
   */
  formal_name: string;

  /**
   * The count of total reports
   */
  report_count: number;

  /**
   * The count of pending reports
   */
  report_pending: number;

  /**
   * The count of total submissions
   */
  submission_count: number;

  /**
   * The count of compilation failed submissions
   */
  submission_failed: number;

  /**
   * The count of compilation succeeded submissions
   */
  submission_succeeded: number;
}
