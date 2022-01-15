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
   * - If Finished, the report is ready
   * - If Analyzing, the report is being generated
   * - If Pending, the report is pending
   */
  state: "Pending" | "Analyzing" | "Finished";

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
   * - If Claimed, this two is marked problematical
   * - If Ignored, the report is ignored
   * - If Unspecified, there is no justification made yet
   */
  justification: "Unspecified" | "Ignored" | "Claimed";
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
