import { PlagiarismSubmission } from "./PlagiarismSubmission";

/**
 * Plagiarism Comparison
 */
export interface PlagiarismComparison {

  /**
   * The report ID
   */
  reportid: string;

  /**
   * The related submission ID
   */
  submitid: number;

  /**
   * The related submission title
   */
  submit: string;

  /**
   * The related exclusive category
   */
  exclusive: number;

  /**
   * Whether the report is generated
   * - If true, the report is ready
   * - If false, the report is being generated
   * - If null, the report is pending
   */
  finished: boolean | null;

  /**
   * The tokens matched
   */
  tokens_matched: number;

  /**
   * The biggest match
   */
  biggest_match: number;

  /**
   * The percent between two submissions
   */
  percent: number;

  /**
   * The percent that this submission is like the related submission
   */
  percent_self: number;

  /**
   * The percent that the related submission is like this submission
   */
  percent_another: number;

  /**
   * The justification result
   * - If true, this two is marked problematical
   * - If false, the report is ignored
   * - If null, there is no justification made yet
   */
  justification: boolean | null;
}

/**
 * Plagiarism Graph Vertex
 */
export interface PlagiarismVertex extends PlagiarismSubmission {

  /**
   * The comparisons from reports.
   */
  comparisons?: PlagiarismComparison[] | null;
}
