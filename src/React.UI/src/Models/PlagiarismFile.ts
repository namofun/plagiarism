/**
 * Plagiarism Set Submission File Entity
 */
export interface PlagiarismFile {

  /**
   * File Id
   */
  fileid: number;

  /**
   * File name with path
   */
  path: string;

  /**
   * File name
   */
  name: string;

  /**
   * File content
   */
  content: string;
}
