// Generated from Les3Parser.ecs by LeMP custom tool. LeMP version: 1.9.0.0
// Note: you can give command-line arguments to the tool via 'Custom Tool Namespace':
// --no-out-header       Suppress this message
// --verbose             Allow verbose messages (shown by VS as 'warnings')
// --timeout=X           Abort processing thread after X seconds (default: 10)
// --macros=FileName.dll Load macros from FileName.dll, path relative to this file 
// Use #importMacros to use macros in a given namespace, e.g. #importMacros(Loyc.LLPG);
using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using Loyc;
using Loyc.Collections;
using Loyc.Syntax.Lexing;
using Loyc.Syntax;
namespace Loyc.Syntax.Les
{
	using TT = TokenType;
	using P = LesPrecedence;
	using S = CodeSymbols;
	partial class Les3Parser
	{
		#pragma warning disable 162, 642
		protected new const TT EOF = TT.EOF;
		void CheckEndMarker(ref TokenType endMarker, ref Token end)
		{
			if (endMarker != end.Type()) {
				if (endMarker == default(TT)) {
					endMarker = end.Type();
				} else {
					Error(-1, "Unexpected separator: {0} should be {1}", ToString(end.TypeInt), ToString((int) endMarker));
				}
			}
		}
		void MissingEndMarker(LNode previousExpr, TokenType endMarker)
		{
			if (previousExpr != null && (TT) LA(-1) == TT.RBrace && (previousExpr.BaseStyle == NodeStyle.Special || previousExpr.BaseStyle == NodeStyle.Statement)) {
			} else {
				Error(0, "Expected '{0}'", endMarker == TT.Comma ? ',' : ';');
			}
		}
		public new VList<LNode> ExprList(VList<LNode> list = default(VList<LNode>))
		{
			var endMarker = default(TT);
			return ExprList(ref endMarker, list);
		}
		protected bool _allowBlockCalls = true;
		new public VList<LNode> StmtList()
		{
			VList<LNode> result = default(VList<LNode>);
			var endMarker = TT.Semicolon;
			result = ExprList(ref endMarker);
			return result;
		}
		public override VList<LNode> ExprList(ref TokenType endMarker, VList<LNode> list = default(VList<LNode>))
		{
			LNode e = default(LNode);
			Token end = default(Token);
			// line 75
			if (LT0.Value is string) {
				endMarker = TT.EOF;
			}
			;
			// Line 1: ( / TopExpr)
			switch ((TT) LA0) {
			case EOF:
			case TT.Comma:
			case TT.RBrace:
			case TT.RBrack:
			case TT.RParen:
			case TT.Semicolon:
				{
				}
				break;
			default:
				e = TopExpr();
				break;
			}
			// Line 81: ((TT.Comma|TT.Semicolon) ( / TopExpr))*
			for (;;) {
				switch ((TT) LA0) {
				case TT.Comma:
				case TT.Semicolon:
					{
						end = MatchAny();
						CheckEndMarker(ref endMarker, ref end);
						list.Add(e ?? MissingExpr());
						e = null;
						// Line 1: ( / TopExpr)
						switch ((TT) LA0) {
						case EOF:
						case TT.Comma:
						case TT.RBrace:
						case TT.RBrack:
						case TT.RParen:
						case TT.Semicolon:
							{
							}
							break;
						default:
							e = TopExpr();
							break;
						}
					}
					break;
				case EOF:
				case TT.RBrace:
				case TT.RBrack:
				case TT.RParen:
					goto stop;
				default:
					{
						MissingEndMarker(e, endMarker);
						list.Add(e ?? MissingExpr());
						e = null;
						// Line 1: ( / TopExpr)
						switch ((TT) LA0) {
						case EOF:
						case TT.Comma:
						case TT.RBrace:
						case TT.RBrack:
						case TT.RParen:
						case TT.Semicolon:
							{
							}
							break;
						default:
							e = TopExpr();
							break;
						}
					}
					break;
				}
			}
		stop:;
			if (e != null || end.Type() == TT.Comma) {
				list.Add(e ?? MissingExpr());
			}
			return list;
		}
		public override IEnumerable<LNode> ExprListLazy(Holder<TokenType> endMarker)
		{
			TT la0;
			LNode e = default(LNode);
			Token end = default(Token);
			// line 94
			if (LT0.Value is string) {
				endMarker = TT.EOF;
			}
			;
			// Line 1: ( / TopExpr)
			la0 = (TT) LA0;
			if (la0 == (TT) EOF || la0 == TT.Comma || la0 == TT.Semicolon) {
			} else
				e = TopExpr();
			// Line 96: ((TT.Comma|TT.Semicolon) ( / TopExpr))*
			for (;;) {
				la0 = (TT) LA0;
				if (la0 == TT.Comma || la0 == TT.Semicolon) {
					end = MatchAny();
					CheckEndMarker(ref endMarker.Value, ref end);
					yield return e ?? MissingExpr();
					e = null;
					// Line 1: ( / TopExpr)
					la0 = (TT) LA0;
					if (la0 == (TT) EOF || la0 == TT.Comma || la0 == TT.Semicolon) {
					} else
						e = TopExpr();
				} else if (la0 == (TT) EOF)
					break;
				else {
					MissingEndMarker(e, endMarker.Value);
					yield return e ?? MissingExpr();
					e = null;
					// Line 1: ( / TopExpr)
					la0 = (TT) LA0;
					if (la0 == (TT) EOF || la0 == TT.Comma || la0 == TT.Semicolon) {
					} else
						e = TopExpr();
				}
			}
			// line 105
			if (e != null || end.Type() == TT.Comma) {
				yield return e ?? MissingExpr();
			}
		}
		new protected LNode TopExpr()
		{
			TT la0;
			LNode e = default(LNode);
			Token litx40 = default(Token);
			// line 108
			var attrs = new VList<LNode>();
			// Line 110: (TT.At Particle)*
			for (;;) {
				la0 = (TT) LA0;
				if (la0 == TT.At) {
					litx40 = MatchAny();
					attrs.Add(Particle(isAttribute: true));
				} else
					break;
			}
			// Line 112: (Expr / KeywordExpression)
			switch ((TT) LA0) {
			case TT.Assignment:
			case TT.BQOperator:
			case TT.Dot:
			case TT.Id:
			case TT.LBrace:
			case TT.LBrack:
			case TT.Literal:
			case TT.LParen:
			case TT.LTokenLiteral:
			case TT.NegativeLiteral:
			case TT.NormalOp:
			case TT.Not:
			case TT.PrefixOp:
			case TT.PreOrSufOp:
				e = Expr(StartStmt);
				break;
			case TT.Keyword:
				e = KeywordExpression();
				break;
			default:
				{
					// line 114
					Error(0, "Expected an expression here");
					MatchExcept();
					// Line 115: nongreedy(~(EOF))*
					for (;;) {
						switch ((TT) LA0) {
						case TT.At:
							{
								switch ((TT) LA(1)) {
								case EOF:
								case TT.Id:
								case TT.LBrace:
								case TT.LBrack:
								case TT.Literal:
								case TT.LParen:
								case TT.LTokenLiteral:
								case TT.NegativeLiteral:
									goto stop;
								default:
									Skip();
									break;
								}
							}
							break;
						case TT.Id:
							{
								switch ((TT) LA(1)) {
								case EOF:
								case TT.Assignment:
								case TT.BQOperator:
								case TT.Comma:
								case TT.Dot:
								case TT.Id:
								case TT.LBrace:
								case TT.LBrack:
								case TT.Literal:
								case TT.LParen:
								case TT.LTokenLiteral:
								case TT.NegativeLiteral:
								case TT.NormalOp:
								case TT.Not:
								case TT.PrefixOp:
								case TT.PreOrSufOp:
								case TT.RBrace:
								case TT.RBrack:
								case TT.RParen:
								case TT.Semicolon:
									goto stop;
								default:
									Skip();
									break;
								}
							}
							break;
						case TT.Assignment:
						case TT.BQOperator:
						case TT.Dot:
						case TT.NormalOp:
						case TT.Not:
						case TT.PrefixOp:
						case TT.PreOrSufOp:
							{
								switch ((TT) LA(1)) {
								case EOF:
								case TT.Assignment:
								case TT.BQOperator:
								case TT.Dot:
								case TT.Id:
								case TT.LBrace:
								case TT.LBrack:
								case TT.Literal:
								case TT.LParen:
								case TT.LTokenLiteral:
								case TT.NegativeLiteral:
								case TT.NormalOp:
								case TT.Not:
								case TT.PrefixOp:
								case TT.PreOrSufOp:
									goto stop;
								default:
									Skip();
									break;
								}
							}
							break;
						case TT.Literal:
						case TT.NegativeLiteral:
							{
								switch ((TT) LA(1)) {
								case TT.Assignment:
								case TT.BQOperator:
								case TT.Dot:
								case TT.LParen:
								case TT.NegativeLiteral:
								case TT.NormalOp:
								case TT.PreOrSufOp:
									goto stop;
								case TT.LBrace:
									{
										if (_allowBlockCalls)
											goto stop;
										else
											Skip();
									}
									break;
								case EOF:
								case TT.Comma:
								case TT.LBrack:
								case TT.Not:
								case TT.RBrace:
								case TT.RBrack:
								case TT.RParen:
								case TT.Semicolon:
									goto stop;
								default:
									Skip();
									break;
								}
							}
							break;
						case TT.LTokenLiteral:
							goto stop;
						case TT.LBrace:
							{
								switch ((TT) LA(1)) {
								case EOF:
								case TT.Assignment:
								case TT.At:
								case TT.BQOperator:
								case TT.Comma:
								case TT.Dot:
								case TT.Id:
								case TT.Keyword:
								case TT.LBrace:
								case TT.LBrack:
								case TT.Literal:
								case TT.LParen:
								case TT.LTokenLiteral:
								case TT.NegativeLiteral:
								case TT.NormalOp:
								case TT.Not:
								case TT.PrefixOp:
								case TT.PreOrSufOp:
								case TT.RBrace:
								case TT.Semicolon:
									goto stop;
								default:
									Skip();
									break;
								}
							}
							break;
						case TT.LBrack:
							{
								switch ((TT) LA(1)) {
								case EOF:
								case TT.Assignment:
								case TT.At:
								case TT.BQOperator:
								case TT.Comma:
								case TT.Dot:
								case TT.Id:
								case TT.Keyword:
								case TT.LBrace:
								case TT.LBrack:
								case TT.Literal:
								case TT.LParen:
								case TT.LTokenLiteral:
								case TT.NegativeLiteral:
								case TT.NormalOp:
								case TT.Not:
								case TT.PrefixOp:
								case TT.PreOrSufOp:
								case TT.RBrack:
								case TT.Semicolon:
									goto stop;
								default:
									Skip();
									break;
								}
							}
							break;
						case TT.LParen:
							{
								switch ((TT) LA(1)) {
								case EOF:
								case TT.Assignment:
								case TT.At:
								case TT.BQOperator:
								case TT.Comma:
								case TT.Dot:
								case TT.Id:
								case TT.Keyword:
								case TT.LBrace:
								case TT.LBrack:
								case TT.Literal:
								case TT.LParen:
								case TT.LTokenLiteral:
								case TT.NegativeLiteral:
								case TT.NormalOp:
								case TT.Not:
								case TT.PrefixOp:
								case TT.PreOrSufOp:
								case TT.RParen:
								case TT.Semicolon:
									goto stop;
								default:
									Skip();
									break;
								}
							}
							break;
						case TT.Keyword:
							{
								switch ((TT) LA(1)) {
								case EOF:
								case TT.Assignment:
								case TT.BQOperator:
								case TT.Dot:
								case TT.Id:
								case TT.LBrace:
								case TT.LBrack:
								case TT.Literal:
								case TT.LParen:
								case TT.LTokenLiteral:
								case TT.NegativeLiteral:
								case TT.NormalOp:
								case TT.Not:
								case TT.PrefixOp:
								case TT.PreOrSufOp:
									goto stop;
								default:
									Skip();
									break;
								}
							}
							break;
						case TT.RBrace:
						case TT.RBrack:
						case TT.RParen:
							{
								switch ((TT) LA(1)) {
								case EOF:
								case TT.Comma:
								case TT.RBrace:
								case TT.RBrack:
								case TT.RParen:
								case TT.Semicolon:
									goto stop;
								default:
									Skip();
									break;
								}
							}
							break;
						case EOF:
							goto stop;
						default:
							Skip();
							break;
						}
					}
				stop:;
					// Line 116: (TopExpr | (EOF|TT.RBrace|TT.RBrack|TT.RParen))
					switch ((TT) LA0) {
					case TT.Assignment:
					case TT.At:
					case TT.BQOperator:
					case TT.Dot:
					case TT.Id:
					case TT.Keyword:
					case TT.LBrace:
					case TT.LBrack:
					case TT.Literal:
					case TT.LParen:
					case TT.LTokenLiteral:
					case TT.NegativeLiteral:
					case TT.NormalOp:
					case TT.Not:
					case TT.PrefixOp:
					case TT.PreOrSufOp:
						e = TopExpr();
						break;
					default:
						{
							Match((int) EOF, (int) TT.RBrace, (int) TT.RBrack, (int) TT.RParen);
							// line 116
							e = MissingExpr();
						}
						break;
					}
				}
				break;
			}
			// line 118
			if (litx40.TypeInt != 0) {
				e = e.WithRange(litx40.StartIndex, e.Range.EndIndex);
			}
			;
			return e.PlusAttrsBefore(attrs);
		}
		LNode KeywordExpression()
		{
			TT la0;
			LNode got_Expr = default(LNode);
			Token kw = default(Token);
			LNode result = default(LNode);
			var old_allowBlockCalls_0 = _allowBlockCalls;
			_allowBlockCalls = false;
			try {
				kw = MatchAny();
				got_Expr = Expr(StartStmt);
				// line 128
				var args = new VList<LNode>(got_Expr);
				// Line 129: (BracesWithContinuators)?
				la0 = (TT) LA0;
				if (la0 == TT.LBrace)
					BracesWithContinuators(ref args);
				result = MarkSpecial(F.Call((Symbol) kw.Value, args, kw.StartIndex, args.Last.Range.EndIndex));
				return result;
			} finally {
				_allowBlockCalls = old_allowBlockCalls_0;
			}
		}
		int BracesWithContinuators(ref VList<LNode> args)
		{
			TT la0, la1;
			LNode bb = default(LNode);
			VList<LNode> got_ExprList = default(VList<LNode>);
			Token kw = default(Token);
			Token lit_rpar = default(Token);
			int endIndex;
			bb = BracedBlock();
			// line 133
			args.Add(bb);
			endIndex = bb.Range.EndIndex;
			// Line 135: greedy(ContinuatorKeyword (BracedBlock | TT.LParen ExprList TT.RParen (BracedBlock / )))*
			for (;;) {
				la0 = (TT) LA0;
				if (la0 == TT.Id) {
					if (Continuators.ContainsKey(LT(0).Value)) {
						la1 = (TT) LA(1);
						if (la1 == TT.LBrace || la1 == TT.LParen) {
							kw = ContinuatorKeyword();
							// line 135
							var opName = Continuators[kw.Value];
							// Line 136: (BracedBlock | TT.LParen ExprList TT.RParen (BracedBlock / ))
							la0 = (TT) LA0;
							if (la0 == TT.LBrace) {
								bb = BracedBlock();
								// line 136
								args.Add(F.Call(opName, bb));
							} else {
								Match((int) TT.LParen);
								got_ExprList = ExprList();
								lit_rpar = Match((int) TT.RParen);
								// Line 138: (BracedBlock / )
								la0 = (TT) LA0;
								if (la0 == TT.LBrace) {
									switch ((TT) LA(1)) {
									case TT.Assignment:
									case TT.At:
									case TT.BQOperator:
									case TT.Comma:
									case TT.Dot:
									case TT.Id:
									case TT.Keyword:
									case TT.LBrace:
									case TT.LBrack:
									case TT.Literal:
									case TT.LParen:
									case TT.LTokenLiteral:
									case TT.NegativeLiteral:
									case TT.NormalOp:
									case TT.Not:
									case TT.PrefixOp:
									case TT.PreOrSufOp:
									case TT.RBrace:
									case TT.Semicolon:
										{
											bb = BracedBlock();
											// line 139
											args.Add(F.Call(opName, got_ExprList.Add(bb), kw.StartIndex, lit_rpar.EndIndex));
										}
										break;
									default:
										// line 140
										args.Add(F.Call(opName, got_ExprList, kw.StartIndex, bb.Range.EndIndex));
										break;
									}
								} else
									// line 140
									args.Add(F.Call(opName, got_ExprList, kw.StartIndex, bb.Range.EndIndex));
							}
						} else
							break;
					} else
						break;
				} else
					break;
			}
			// line 143
			return endIndex;
		}
		LNode BracedBlock()
		{
			Token lit_lcub = default(Token);
			Token lit_rcub = default(Token);
			VList<LNode> stmts = default(VList<LNode>);
			lit_lcub = Match((int) TT.LBrace);
			stmts = StmtList();
			lit_rcub = Match((int) TT.RBrace);
			// line 146
			return F.Call(S.Braces, stmts, lit_lcub.StartIndex, lit_rcub.EndIndex);
		}
		LNode Parentheses()
		{
			VList<LNode> exprs = default(VList<LNode>);
			Token lit_lpar = default(Token);
			Token lit_rpar = default(Token);
			var endMarker = default(TokenType);
			lit_lpar = Match((int) TT.LParen);
			exprs = ExprList(ref endMarker);
			lit_rpar = Match((int) TT.RParen);
			// line 149
			return exprs.Count == 1 && endMarker != TT.Semicolon ? exprs[0] : F.Tuple(exprs, lit_lpar.StartIndex, lit_rpar.EndIndex);
		}
		Token ContinuatorKeyword()
		{
			Token result = default(Token);
			result = MatchAny();
			return result;
		}
		LNode Expr(Precedence context)
		{
			TT la0;
			LNode e = default(LNode);
			Token op = default(Token);
			LNode result = default(LNode);
			Token t = default(Token);
			// Line 168: (&{context.CanParse(P.Juxtaposition)} &{LA($LI + 1).IsOneOf((int) TT.Id, (int) TT.PrefixOp, (int) TT.Literal)} TT.Id Expr / PrefixExpr greedy( &{context.CanParse(prec = InfixPrecedenceOf(LT($LI)))} (TT.Assignment|TT.BQOperator|TT.Dot|TT.NormalOp) Expr | &{context.CanParse(P.Add)} TT.NegativeLiteral | &{context.CanParse(SuffixPrecedenceOf(LT($LI)))} TT.PreOrSufOp | &{context.CanParse(P.Primary)} FinishPrimaryExpr )*)
			do {
				la0 = (TT) LA0;
				if (la0 == TT.Id) {
					if (context.CanParse(P.Juxtaposition)) {
						if (LA(0 + 1).IsOneOf((int) TT.Id, (int) TT.PrefixOp, (int) TT.Literal)) {
							var kw = MatchAny();
							var rhs = Expr(P.Juxtaposition);
							// line 172
							result = F.Call((Symbol) kw.Value, rhs);
						} else
							goto matchPrefixExpr;
					} else
						goto matchPrefixExpr;
				} else
					goto matchPrefixExpr;
				break;
			matchPrefixExpr:
				{
					// line 174
					Precedence prec;
					e = PrefixExpr(context);
					// Line 178: greedy( &{context.CanParse(prec = InfixPrecedenceOf(LT($LI)))} (TT.Assignment|TT.BQOperator|TT.Dot|TT.NormalOp) Expr | &{context.CanParse(P.Add)} TT.NegativeLiteral | &{context.CanParse(SuffixPrecedenceOf(LT($LI)))} TT.PreOrSufOp | &{context.CanParse(P.Primary)} FinishPrimaryExpr )*
					for (;;) {
						switch ((TT) LA0) {
						case TT.Assignment:
						case TT.BQOperator:
						case TT.Dot:
						case TT.NormalOp:
							{
								if (context.CanParse(prec = InfixPrecedenceOf(LT(0)))) {
									// line 179
									if (!prec.CanMixWith(context)) {
										Error(0, "Operator '{0}' is not allowed in this context. Add parentheses to clarify the code's meaning.", LT0.Value);
									}
									;
									op = MatchAny();
									var rhs = Expr(prec);
									// line 184
									e = F.Call((Symbol) op.Value, e, rhs, e.Range.StartIndex, rhs.Range.EndIndex).SetStyle(NodeStyle.Operator);
								} else
									goto stop;
							}
							break;
						case TT.NegativeLiteral:
							{
								if (context.CanParse(P.Add)) {
									var rhs = MatchAny();
									// line 187
									e = F.Call(S.Sub, e, ToPositiveLiteral(rhs), e.Range.StartIndex, rhs.EndIndex).SetStyle(NodeStyle.Operator);
								} else
									goto stop;
							}
							break;
						case TT.PreOrSufOp:
							{
								if (context.CanParse(SuffixPrecedenceOf(LT(0)))) {
									t = MatchAny();
									// line 191
									e = F.Call(ToSuffixOpName((Symbol) t.Value), e, e.Range.StartIndex, t.EndIndex).SetStyle(NodeStyle.Operator);
								} else
									goto stop;
							}
							break;
						case TT.LParen:
							{
								if (context.CanParse(P.Primary))
									e = FinishPrimaryExpr(e);
								else
									goto stop;
							}
							break;
						case TT.LBrace:
							{
								if (_allowBlockCalls) {
									if (context.CanParse(P.Primary))
										e = FinishPrimaryExpr(e);
									else
										goto stop;
								} else
									goto stop;
							}
							break;
						case TT.LBrack:
						case TT.Not:
							{
								if (context.CanParse(P.Primary))
									e = FinishPrimaryExpr(e);
								else
									goto stop;
							}
							break;
						default:
							goto stop;
						}
					}
				stop:;
					// line 196
					return e;
				}
			} while (false);
			return result;
		}
		LNode FinishPrimaryExpr(LNode e)
		{
			TT la0;
			LNode result = default(LNode);
			// Line 201: ( CallArgs | TT.Not (TT.LParen ExprList TT.RParen / Expr) | TT.LBrack ExprList TT.RBrack )
			la0 = (TT) LA0;
			if (la0 == TT.LBrace || la0 == TT.LParen)
				result = CallArgs(e);
			else if (la0 == TT.Not) {
				Skip();
				// line 204
				var args = new VList<LNode> { 
					e
				};
				int endIndex;
				// Line 205: (TT.LParen ExprList TT.RParen / Expr)
				la0 = (TT) LA0;
				if (la0 == TT.LParen) {
					Skip();
					args = ExprList(args);
					var c = Match((int) TT.RParen);
					// line 205
					endIndex = c.EndIndex;
				} else {
					var T = Expr(P.Primary);
					// line 206
					args.Add(T);
					endIndex = T.Range.EndIndex;
				}
				// line 208
				return F.Call(S.Of, args, e.Range.StartIndex, endIndex).SetStyle(NodeStyle.Operator);
			} else {
				// line 210
				var args = new VList<LNode> { 
					e
				};
				Match((int) TT.LBrack);
				args = ExprList(args);
				var c = Match((int) TT.RBrack);
				// line 212
				return F.Call(S.IndexBracks, args, e.Range.StartIndex, c.EndIndex).SetStyle(NodeStyle.Operator);
			}
			return result;
		}
		LNode CallArgs(LNode target)
		{
			TT la0;
			VList<LNode> args = default(VList<LNode>);
			int endIndex = 0;
			Token lit_rpar = default(Token);
			LNode result = default(LNode);
			var endMarker = default(TokenType);
			bool hasBraces = false;
			// Line 218: (TT.LParen ExprList TT.RParen greedy(&{_allowBlockCalls} BracesWithContinuators)? | &{_allowBlockCalls} BracesWithContinuators)
			la0 = (TT) LA0;
			if (la0 == TT.LParen) {
				Skip();
				args = ExprList(ref endMarker);
				lit_rpar = Match((int) TT.RParen);
				// line 218
				endIndex = lit_rpar.EndIndex;
				// Line 220: greedy(&{_allowBlockCalls} BracesWithContinuators)?
				la0 = (TT) LA0;
				if (la0 == TT.LBrace) {
					if (_allowBlockCalls) {
						switch ((TT) LA(1)) {
						case TT.Assignment:
						case TT.At:
						case TT.BQOperator:
						case TT.Comma:
						case TT.Dot:
						case TT.Id:
						case TT.Keyword:
						case TT.LBrace:
						case TT.LBrack:
						case TT.Literal:
						case TT.LParen:
						case TT.LTokenLiteral:
						case TT.NegativeLiteral:
						case TT.NormalOp:
						case TT.Not:
						case TT.PrefixOp:
						case TT.PreOrSufOp:
						case TT.RBrace:
						case TT.Semicolon:
							{
								endIndex = BracesWithContinuators(ref args);
								// line 220
								hasBraces = true;
							}
							break;
						}
					}
				}
			} else {
				Check(_allowBlockCalls, "_allowBlockCalls");
				endIndex = BracesWithContinuators(ref args);
				// line 221
				hasBraces = true;
			}
			// line 223
			result = F.Call(target, args, target.Range.StartIndex, endIndex).SetBaseStyle(NodeStyle.PrefixNotation);
			if (hasBraces)
				MarkSpecial(result);
			if (endMarker == TT.Semicolon) {
				result.Style |= NodeStyle.Alternate;
			}
			;
			return result;
		}
		LNode PrefixExpr(Precedence context)
		{
			LNode e = default(LNode);
			LNode result = default(LNode);
			Token t = default(Token);
			// Line 230: ((TT.Assignment|TT.BQOperator|TT.Dot|TT.NormalOp|TT.Not|TT.PrefixOp|TT.PreOrSufOp) Expr | Particle)
			switch ((TT) LA0) {
			case TT.Assignment:
			case TT.BQOperator:
			case TT.Dot:
			case TT.NormalOp:
			case TT.Not:
			case TT.PrefixOp:
			case TT.PreOrSufOp:
				{
					t = MatchAny();
					e = Expr(PrefixPrecedenceOf(t));
					// line 232
					result = F.Call((Symbol) t.Value, e, t.StartIndex, e.Range.EndIndex).SetStyle(NodeStyle.Operator);
				}
				break;
			default:
				result = Particle();
				break;
			}
			return result;
		}
		LNode Particle(bool isAttribute = false)
		{
			Token c = default(Token);
			Token lit_apos_lcub = default(Token);
			Token lit_lcub = default(Token);
			Token lit_lpar = default(Token);
			Token lit_lsqb = default(Token);
			Token lit_rcub = default(Token);
			Token lit_rpar = default(Token);
			Token lit_rsqb = default(Token);
			LNode result = default(LNode);
			TokenTree tree = default(TokenTree);
			// Line 245: ( TT.Id | (TT.Literal|TT.NegativeLiteral) | TT.LTokenLiteral TokenTree TT.RBrace | TT.LBrace StmtList TT.RBrace | TT.LBrack ExprList TT.RBrack | TT.LParen ExprList TT.RParen )
			switch ((TT) LA0) {
			case TT.Id:
				{
					var id = MatchAny();
					// line 246
					result = F.Id(id).SetStyle(id.Style);
				}
				break;
			case TT.Literal:
			case TT.NegativeLiteral:
				{
					var lit = MatchAny();
					// line 248
					result = F.Literal(lit);
				}
				break;
			case TT.LTokenLiteral:
				{
					lit_apos_lcub = MatchAny();
					tree = TokenTree();
					c = Match((int) TT.RBrace);
					// line 251
					result = F.Literal(tree, lit_apos_lcub.StartIndex, c.EndIndex);
				}
				break;
			case TT.LBrace:
				{
					lit_lcub = MatchAny();
					var list = StmtList();
					lit_rcub = Match((int) TT.RBrace);
					// line 254
					result = F.Braces(list, lit_lcub.StartIndex, lit_rcub.EndIndex).SetStyle(NodeStyle.Statement);
				}
				break;
			case TT.LBrack:
				{
					lit_lsqb = MatchAny();
					var list = ExprList();
					lit_rsqb = Match((int) TT.RBrack);
					// line 257
					result = F.Call(S.Array, list, lit_lsqb.StartIndex, lit_rsqb.EndIndex).SetStyle(NodeStyle.Expression);
				}
				break;
			case TT.LParen:
				{
					// line 259
					var endMarker = default(TT);
					lit_lpar = MatchAny();
					// line 260
					var saveParens = !isAttribute && (TT) LA0 != TT.At;
					var list = ExprList(ref endMarker);
					lit_rpar = Match((int) TT.RParen);
					// line 263
					if (endMarker == TT.Semicolon || list.Count != 1) {
						result = F.Call(S.Tuple, list, lit_lpar.StartIndex, lit_rpar.EndIndex);
						if (endMarker == TT.Comma) {
							var msg = "Tuples require ';' as a separator.";
							ErrorSink.Write(Severity.Error, list[0].Range.End, msg);
						}
						;
					} else {
						result = saveParens ? F.InParens(list[0], lit_lpar.StartIndex, lit_rpar.EndIndex) : list[0];
					}
					;
				}
				break;
			default:
				{
					// line 274
					Error(0, "Expected a particle (id, literal, {braces} or (parens)).");
					result = MissingExpr();
				}
				break;
			}
			return result;
		}
		TokenTree TokenTree()
		{
			TT la1;
			TokenTree got_TokenTree = default(TokenTree);
			TokenTree result = default(TokenTree);
			result = new TokenTree(SourceFile);
			// Line 281: nongreedy((TT.LBrace|TT.LBrack|TT.LParen) TokenTree (TT.RBrace|TT.RBrack|TT.RParen) / ~(EOF))*
			for (;;) {
				switch ((TT) LA0) {
				case EOF:
				case TT.RBrace:
				case TT.RBrack:
				case TT.RParen:
					goto stop;
				case TT.LBrace:
				case TT.LBrack:
				case TT.LParen:
					{
						la1 = (TT) LA(1);
						if (la1 != (TT) EOF) {
							var open = MatchAny();
							got_TokenTree = TokenTree();
							// line 283
							result.Add(open.WithValue(got_TokenTree));
							result.Add(Match((int) TT.RBrace, (int) TT.RBrack, (int) TT.RParen));
						} else
							result.Add(MatchAny());
					}
					break;
				default:
					result.Add(MatchAny());
					break;
				}
			}
		stop:;
			return result;
		}
	}
}
