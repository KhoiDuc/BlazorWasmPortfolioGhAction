using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using BlazorWasmPortfolioGhAction.Pages;

namespace BlazorWasmPortfolioGhAction.Tests;

public static class ToeicWordFamilyBuilder
{
    private static readonly Dictionary<string, (string? v, string? n, string? adj, string? adv)> CuratedFamilies =
        new(StringComparer.OrdinalIgnoreCase)
    {
        // Business Administration & Management
        ["negotiate"] = ("negotiate", "negotiation, negotiator", "negotiable", "negotiably"),
        ["implement"] = ("implement", "implementation", "implementable, implemented", null),
        ["comply with"] = ("comply", "compliance", "compliant", "compliantly"),
        ["comply"] = ("comply", "compliance", "compliant", "compliantly"),
        ["compliance"] = ("comply", "compliance", "compliant", "compliantly"),
        ["delegate"] = ("delegate", "delegation, delegate", "delegated", null),
        ["mandatory"] = ("mandate", "mandate", "mandatory", "mandatorily"),
        ["unanimous"] = (null, "unanimity", "unanimous", "unanimously"),
        ["acquire"] = ("acquire", "acquisition, acquirer", "acquired, acquisitive", null),
        ["acquisition"] = ("acquire", "acquisition, acquirer", "acquired", null),
        ["merger"] = ("merge", "merger", "merged", null),
        ["merge"] = ("merge", "merger", "merged", null),
        ["stakeholder"] = ("stake", "stakeholder, stake", null, null),
        ["shareholder"] = ("share", "shareholder, share", null, null),
        ["board of directors"] = ("direct", "direction, director, board", "direct", "directly"),
        ["executive"] = ("execute", "execution, executive", "executive, executable", "executively"),
        ["execute"] = ("execute", "execution, executive", "executable", "executively"),
        ["oversee"] = ("oversee", "oversight, overseer", null, null),
        ["oversight"] = ("oversee", "oversight", null, null),
        ["authorize"] = ("authorize", "authorization, authority", "authorized, authoritative", "authoritatively"),
        ["approve"] = ("approve", "approval", "approved, approving", "approvingly"),
        ["approval"] = ("approve", "approval", "approved", null),
        ["allocate"] = ("allocate", "allocation", "allocated", null),
        ["allocation"] = ("allocate", "allocation", "allocated", null),
        ["streamline"] = ("streamline", "streamline", "streamlined", null),
        ["downsize"] = ("downsize", "downsizing", "downsized", null),
        ["restructure"] = ("restructure", "restructuring, structure", "structural", "structurally"),
        ["incorporate"] = ("incorporate", "incorporation", "incorporated", null),
        ["dissolve"] = ("dissolve", "dissolution", "dissolved", null),
        ["franchise"] = ("franchise", "franchise, franchisee, franchisor", "franchised", null),
        ["subsidiary"] = (null, "subsidiary", "subsidiary", null),
        ["affiliate"] = ("affiliate", "affiliation, affiliate", "affiliated", null),
        ["joint venture"] = ("venture", "venture", "ventured", null),
        ["consensus"] = ("consent", "consensus, consent", "consensual", "consensually"),
        ["ballot"] = ("ballot", "ballot", null, null),
        ["proxy"] = (null, "proxy", null, null),
        ["bylaws"] = (null, "bylaws, law", "lawful", "lawfully"),
        ["agenda"] = (null, "agenda", null, null),
        ["minutes"] = ("minute", "minutes", null, null),
        ["resolution"] = ("resolve", "resolution", "resolute, resolved", "resolutely"),
        ["resolve"] = ("resolve", "resolution", "resolved", null),
        ["motion"] = ("move", "motion, movement", "moving, movable", null),
        ["adjourn"] = ("adjourn", "adjournment", "adjourned", null),
        ["ratify"] = ("ratify", "ratification", "ratified", null),
        ["amend"] = ("amend", "amendment", "amended", null),
        ["amendment"] = ("amend", "amendment", "amended", null),
        ["regulation"] = ("regulate", "regulation, regulator", "regulatory, regulated", null),
        ["regulate"] = ("regulate", "regulation, regulator", "regulatory", null),
        ["statute"] = (null, "statute", "statutory", "statutorily"),
        ["ordinance"] = ("ordain", "ordinance", null, null),
        ["mandate"] = ("mandate", "mandate", "mandatory", "mandatorily"),
        ["waive"] = ("waive", "waiver", "waived", null),
        ["waiver"] = ("waive", "waiver", null, null),
        ["relinquish"] = ("relinquish", "relinquishment", "relinquished", null),
        ["abdicate"] = ("abdicate", "abdication", "abdicated", null),
        ["successor"] = ("succeed", "succession, successor", "successive, succeeding", "successively"),
        ["predecessor"] = ("precede", "predecessor, precedence", "preceding", null),
        ["tenure"] = (null, "tenure", "tenured", null),
        ["remuneration"] = ("remunerate", "remuneration", "remunerative", null),
        ["severance"] = ("sever", "severance", "severable, severed", null),

        // Finance & Accounting & Banking
        ["liquidity"] = ("liquidate", "liquidity, liquidation, liquid", "liquid", "liquidly"),
        ["collateral"] = (null, "collateral", "collateral", "collaterally"),
        ["equity"] = (null, "equity", "equitable", "equitably"),
        ["depreciation"] = ("depreciate", "depreciation", "depreciable, depreciated", null),
        ["amortization"] = ("amortize", "amortization", "amortized", null),
        ["portfolio"] = (null, "portfolio", null, null),
        ["hedge"] = ("hedge", "hedge, hedging", "hedged", null),
        ["yield"] = ("yield", "yield", "yielding", null),
        ["speculate"] = ("speculate", "speculation, speculator", "speculative", "speculatively"),
        ["bullish"] = (null, "bull", "bullish", "bullishly"),
        ["bearish"] = (null, "bear", "bearish", "bearishly"),
        ["inflation"] = ("inflate", "inflation", "inflationary, inflated", null),
        ["recession"] = ("recede", "recession", "recessionary", null),
        ["fiscal"] = (null, "fiscality", "fiscal", "fiscally"),
        ["deficit"] = (null, "deficit, deficiency", "deficient", "defiantly"),
        ["surplus"] = (null, "surplus", "surplus", null),
        ["liability"] = (null, "liability", "liable", null),
        ["asset"] = (null, "asset", null, null),
        ["revenue"] = (null, "revenue", null, null),
        ["dividend"] = ("divide", "dividend, division", "divided, divisible", null),
        ["capital"] = ("capitalize", "capital, capitalization", "capital", "capitally"),
        ["leverage"] = ("leverage", "leverage", "leveraged", null),
        ["solvent"] = (null, "solvency", "solvent", null),
        ["default"] = ("default", "default, defaulter", "defaulted", null),
        ["bankrupt"] = ("bankrupt", "bankruptcy", "bankrupt", null),
        ["bankruptcy"] = ("bankrupt", "bankruptcy", "bankrupt", null),
        ["audit"] = ("audit", "audit, auditor", "audited", null),
        ["auditor"] = ("audit", "audit, auditor", "audited", null),
        ["ledger"] = (null, "ledger", null, null),
        ["accrual"] = ("accrue", "accrual", "accrued", null),
        ["accrue"] = ("accrue", "accrual", "accrued", null),
        ["deduction"] = ("deduct", "deduction", "deductible", null),
        ["deduct"] = ("deduct", "deduction", "deductible", null),
        ["deductible"] = ("deduct", "deduction, deductible", "deductible", null),
        ["withholding"] = ("withhold", "withholding", "withheld", null),
        ["withhold"] = ("withhold", "withholding", null, null),
        ["exemption"] = ("exempt", "exemption", "exempt", null),
        ["exempt"] = ("exempt", "exemption", "exempt", null),
        ["penalty"] = ("penalize", "penalty", "penal", null),
        ["penalize"] = ("penalize", "penalty", "penal", null),
        ["refund"] = ("refund", "refund", "refundable", null),
        ["evasion"] = ("evade", "evasion", "evasive", "evasively"),
        ["evade"] = ("evade", "evasion", "evasive", "evasively"),

        // Marketing, Sales & Advertising
        ["advertise"] = ("advertise", "advertisement, advertiser, advertising", "advertised", null),
        ["advertisement"] = ("advertise", "advertisement, advertiser", null, null),
        ["endorse"] = ("endorse", "endorsement, endorser", "endorsed", null),
        ["endorsement"] = ("endorse", "endorsement", "endorsed", null),
        ["campaign"] = ("campaign", "campaign, campaigner", null, null),
        ["demographic"] = (null, "demographics, demography", "demographic", "demographically"),
        ["segmentation"] = ("segment", "segment, segmentation", "segmented", null),
        ["segment"] = ("segment", "segment, segmentation", "segmented", null),
        ["penetration"] = ("penetrate", "penetration", "penetrating", null),
        ["penetrate"] = ("penetrate", "penetration", "penetrated", null),
        ["flyer"] = ("fly", "flyer, flight", null, null),
        ["brochure"] = (null, "brochure", null, null),
        ["billboard"] = (null, "billboard", null, null),
        ["commercial"] = ("commercialize", "commercial, commerce, commercialization", "commercial", "commercially"),
        ["sponsorship"] = ("sponsor", "sponsor, sponsorship", "sponsored", null),
        ["sponsor"] = ("sponsor", "sponsor, sponsorship", "sponsored", null),
        ["prospect"] = ("prospect", "prospect, prospector, prospecting", "prospective", "prospectively"),
        ["prospecting"] = ("prospect", "prospect, prospecting", "prospective", null),
        ["discount"] = ("discount", "discount", "discounted", null),
        ["quota"] = (null, "quota", null, null),
        ["commission"] = ("commission", "commission, commissioner", "commissioned", null),
        ["pitch"] = ("pitch", "pitch", null, null),
        ["rebate"] = ("rebate", "rebate", null, null),
        ["invoice"] = ("invoice", "invoice", "invoiced", null),
        ["quote"] = ("quote", "quote, quotation", "quoted", null),
        ["quotation"] = ("quote", "quotation", "quoted", null),
        ["turnover"] = ("turn over", "turnover", null, null),
        ["retain"] = ("retain", "retention, retainer", "retentive", null),
        ["retention"] = ("retain", "retention", "retentive", null),
        ["churn"] = ("churn", "churn, churning", null, null),

        // HR, Employment & Workplace
        ["recruit"] = ("recruit", "recruitment, recruiter", "recruited", null),
        ["recruitment"] = ("recruit", "recruitment, recruiter", null, null),
        ["applicant"] = ("apply", "application, applicant", "applicable, applied", null),
        ["apply"] = ("apply", "application, applicant", "applicable", "applicably"),
        ["application"] = ("apply", "application, applicant", "applicable", null),
        ["candidate"] = (null, "candidate, candidacy", null, null),
        ["resume"] = ("resume", "resume", null, null),
        ["curriculum vitae"] = (null, "curriculum vitae", null, null),
        ["interview"] = ("interview", "interview, interviewer, interviewee", null, null),
        ["probation"] = (null, "probation", "probationary", "probationarily"),
        ["appraisal"] = ("appraise", "appraisal, appraiser", "appraised", null),
        ["appraise"] = ("appraise", "appraisal, appraiser", "appraised", null),
        ["compensation"] = ("compensate", "compensation", "compensatory", null),
        ["compensate"] = ("compensate", "compensation", "compensatory", null),
        ["benefit"] = ("benefit", "benefit, beneficiary", "beneficial", "beneficially"),
        ["beneficial"] = ("benefit", "benefit", "beneficial", "beneficially"),
        ["incentive"] = ("incentivize", "incentive", "incentive", null),
        ["incentivize"] = ("incentivize", "incentive", "incentivized", null),
        ["pension"] = ("pension", "pension, pensioner", "pensionable", null),
        ["vacancy"] = (null, "vacancy", "vacant", "vacantly"),
        ["vacant"] = (null, "vacancy", "vacant", "vacantly"),
        ["overtime"] = (null, "overtime", "overtime", null),
        ["payroll"] = (null, "payroll", null, null),
        ["terminate"] = ("terminate", "termination", "terminated", null),
        ["termination"] = ("terminate", "termination", "terminated", null),
        ["layoff"] = ("lay off", "layoff", null, null),
        ["resign"] = ("resign", "resignation", "resigned", "resignedly"),
        ["resignation"] = ("resign", "resignation", "resigned", null),
        ["retire"] = ("retire", "retirement, retiree", "retired", null),
        ["retirement"] = ("retire", "retirement, retiree", "retired", null),
        ["promote"] = ("promote", "promotion, promoter", "promotional", "promotionally"),
        ["promotion"] = ("promote", "promotion, promoter", "promotional", null),
        ["demote"] = ("demote", "demotion", "demoted", null),
        ["transfer"] = ("transfer", "transfer, transference", "transferable", null),
        ["absentee"] = (null, "absence, absentee, absenteeism", "absent", "absently"),
        ["absence"] = (null, "absence", "absent", "absently"),

        // Contracts & Legal
        ["contract"] = ("contract", "contract, contractor", "contractual", "contractually"),
        ["contractor"] = ("contract", "contract, contractor", "contractual", null),
        ["clause"] = (null, "clause", null, null),
        ["stipulation"] = ("stipulate", "stipulation", "stipulated", null),
        ["stipulate"] = ("stipulate", "stipulation", "stipulated", null),
        ["binding"] = ("bind", "bond, binding", "binding, bound", null),
        ["bind"] = ("bind", "bond, binding", "binding", null),
        ["breach"] = ("breach", "breach", "breached", null),
        ["null and void"] = ("nullify", "nullity", "null, void", null),
        ["obligation"] = ("oblige, obligate", "obligation", "obligatory, obligated", "obligatorily"),
        ["obligate"] = ("obligate", "obligation", "obligatory", null),
        ["provision"] = ("provide", "provision, provider", "provisional", "provisionally"),
        ["warranty"] = ("warrant", "warranty, warrant", "warranted", null),
        ["indemnify"] = ("indemnify", "indemnity, indemnification", "indemnified", null),
        ["indemnity"] = ("indemnify", "indemnity", "indemnified", null),
        ["arbitration"] = ("arbitrate", "arbitration, arbitrator", "arbitrated", null),
        ["arbitrate"] = ("arbitrate", "arbitration, arbitrator", "arbitrary", "arbitrarily"),
        ["jurisdiction"] = (null, "jurisdiction", "jurisdictional", "jurisdictionally"),
        ["liability"] = (null, "liability", "liable", null),
        ["confidential"] = ("confide", "confidence, confidentiality", "confidential, confident", "confidentially, confidently"),
        ["confidentiality"] = ("confide", "confidentiality", "confidential", "confidentially"),
        ["severable"] = ("sever", "severance, severability", "severable", null),
        ["force majeure"] = (null, "force majeure", null, null),

        // Shipping, Logistics & Travel
        ["freight"] = ("freight", "freight, freighter", "freighted", null),
        ["cargo"] = (null, "cargo", null, null),
        ["customs"] = (null, "customs, custom", "customary", "customarily"),
        ["tariff"] = (null, "tariff", null, null),
        ["carrier"] = ("carry", "carrier, carriage", null, null),
        ["dispatch"] = ("dispatch", "dispatch, dispatcher", "dispatched", null),
        ["expedite"] = ("expedite", "expedition, expediter", "expeditious", "expeditiously"),
        ["warehouse"] = ("warehouse", "warehouse, warehousing", null, null),
        ["inventory"] = ("inventory", "inventory", null, null),
        ["manifest"] = ("manifest", "manifest, manifestation", "manifest", "manifestly"),
        ["consignment"] = ("consign", "consignment, consignee, consignor", null, null),
        ["consign"] = ("consign", "consignment", null, null),
        ["clearance"] = ("clear", "clearance", "clear", "clearly"),
        ["transit"] = ("transit", "transit, transition", "transitory, transitional", "transitively"),
        ["itinerary"] = (null, "itinerary", "itinerant", null),
        ["concierge"] = (null, "concierge", null, null),
        ["valet"] = ("valet", "valet", null, null),
        ["amenities"] = (null, "amenity, amenities", null, null),
        ["reservation"] = ("reserve", "reservation", "reserved", null),
        ["reserve"] = ("reserve", "reservation", "reserved", null),
        ["accommodation"] = ("accommodate", "accommodation", "accommodating", null),
        ["accommodate"] = ("accommodate", "accommodation", "accommodating", null),
        ["airport shuttle"] = ("shuttle", "shuttle", null, null),
        ["layover"] = ("lay over", "layover", null, null),
        ["departure"] = ("depart", "departure", "departing, departed", null),
        ["depart"] = ("depart", "departure", "departed", null),
        ["arrival"] = ("arrive", "arrival", "arrived", null),
        ["arrive"] = ("arrive", "arrival", "arrived", null),
        ["boarding pass"] = ("board", "boarding, pass", null, null),
        ["baggage"] = (null, "baggage, bag", null, null),
        ["complimentary"] = ("compliment", "compliment", "complimentary", "complimentarily"),

        // IT, Software, Telecom & Analytics
        ["bandwidth"] = (null, "bandwidth", null, null),
        ["latency"] = (null, "latency", "latent", "latently"),
        ["firewall"] = (null, "firewall", null, null),
        ["encryption"] = ("encrypt", "encryption", "encrypted", null),
        ["encrypt"] = ("encrypt", "encryption", "encrypted", null),
        ["credential"] = (null, "credentials", null, null),
        ["authenticate"] = ("authenticate", "authentication, authenticity", "authentic", "authentically"),
        ["authentication"] = ("authenticate", "authentication", "authentic", null),
        ["database"] = (null, "database", null, null),
        ["backup"] = ("back up", "backup", null, null),
        ["server"] = ("serve", "server, service", "serviceable", null),
        ["cloud"] = ("cloud", "cloud", "cloudy", null),
        ["deployment"] = ("deploy", "deployment", "deployed", null),
        ["deploy"] = ("deploy", "deployment", "deployed", null),
        ["infrastructure"] = (null, "infrastructure", "infrastructural", "infrastructurally"),
        ["teleconference"] = ("teleconference", "teleconference", null, null),
        ["webinar"] = (null, "webinar", null, null),
        ["analytics"] = ("analyze", "analytics, analysis, analyst", "analytical", "analytically"),
        ["analyze"] = ("analyze", "analysis, analyst", "analytical", "analytically"),
        ["analysis"] = ("analyze", "analysis, analyst", "analytical", "analytically"),
        ["benchmark"] = ("benchmark", "benchmark, benchmarking", null, null),
        ["dashboard"] = (null, "dashboard", null, null),
        ["algorithm"] = (null, "algorithm", "algorithmic", "algorithmically"),
        ["predictive"] = ("predict", "prediction, predictor", "predictive, predictable", "predictably"),
        ["predict"] = ("predict", "prediction", "predictive, predictable", "predictably"),
        ["agile"] = (null, "agility", "agile", "agilely"),

        // Facilities, Manufacturing & Product Dev
        ["blueprint"] = ("blueprint", "blueprint", null, null),
        ["prototype"] = ("prototype", "prototype, prototyping", "prototypical", "prototypically"),
        ["assembly"] = ("assemble", "assembly, assembler", "assembled", null),
        ["assemble"] = ("assemble", "assembly", "assembled", null),
        ["maintenance"] = ("maintain", "maintenance, maintainer", "maintainable", null),
        ["maintain"] = ("maintain", "maintenance", "maintainable", null),
        ["renovation"] = ("renovate", "renovation, renovator", "renovated", null),
        ["renovate"] = ("renovate", "renovation", "renovated", null),
        ["contractor"] = ("contract", "contractor, contract", "contractual", null),
        ["inspection"] = ("inspect", "inspection, inspector", "inspected", null),
        ["inspect"] = ("inspect", "inspection, inspector", "inspected", null),
        ["occupancy"] = ("occupy", "occupancy, occupant, occupation", "occupied, occupational", "occupationally"),
        ["occupy"] = ("occupy", "occupancy, occupant", "occupied", null),
        ["premises"] = (null, "premises", null, null),
        ["specification"] = ("specify", "specification, spec", "specific", "specifically"),
        ["specify"] = ("specify", "specification", "specific", "specifically"),
        ["defect"] = (null, "defect, defection", "defective", "defectively"),
        ["defective"] = (null, "defect", "defective", "defectively"),
        ["warranty"] = ("warrant", "warranty", "warranted", null),
        ["efficiency"] = (null, "efficiency", "efficient", "efficiently"),
        ["efficient"] = (null, "efficiency", "efficient", "efficiently"),
        ["throughput"] = (null, "throughput", null, null),
        ["custodial"] = (null, "custody, custodian", "custodial", null),
        ["janitorial"] = (null, "janitor", "janitorial", null),

        // Quality, Performance, Research & Innovation
        ["innovate"] = ("innovate", "innovation, innovator", "innovative", "innovatively"),
        ["innovation"] = ("innovate", "innovation, innovator", "innovative", "innovatively"),
        ["collaborate"] = ("collaborate", "collaboration, collaborator", "collaborative", "collaboratively"),
        ["collaboration"] = ("collaborate", "collaboration", "collaborative", "collaboratively"),
        ["evaluate"] = ("evaluate", "evaluation, evaluator", "evaluative", null),
        ["evaluation"] = ("evaluate", "evaluation", "evaluative", null),
        ["assess"] = ("assess", "assessment, assessor", "assessable", null),
        ["assessment"] = ("assess", "assessment", "assessable", null),
        ["competence"] = ("compete", "competence, competency, competition, competitor", "competent, competitive", "competently, competitively"),
        ["competent"] = ("compete", "competence", "competent", "competently"),
        ["compete"] = ("compete", "competition, competitor", "competitive", "competitively"),
        ["competition"] = ("compete", "competition, competitor", "competitive", "competitively"),
        ["competitive"] = ("compete", "competition", "competitive", "competitively"),
        ["milestone"] = (null, "milestone", null, null),
        ["initiative"] = ("initiate", "initiative, initiation, initiator", "initial, initiative", "initially"),
        ["initiate"] = ("initiate", "initiative, initiation", "initial", "initially"),
        ["productive"] = ("produce", "production, productivity, producer, product", "productive", "productively"),
        ["produce"] = ("produce", "production, product, producer", "productive", "productively"),
        ["productivity"] = ("produce", "productivity, production", "productive", "productively"),
        ["strategy"] = ("strategize", "strategy, strategist", "strategic", "strategically"),
        ["strategic"] = ("strategize", "strategy", "strategic", "strategically"),
        ["feedback"] = ("feed back", "feedback", null, null),
        ["objective"] = (null, "objective, objectivity", "objective", "objectively"),

        // Confusable Words
        ["beside vs besides"] = (null, null, "beside", "beside, besides"),

        // Collocations & Phrasal Expressions
        ["take advantage of"] = ("take advantage of, exploit", "advantage", "advantageous", "advantageously"),
        ["under construction"] = ("construct", "construction", "under construction, constructive", "constructively"),
        ["draw a conclusion"] = ("conclude, draw a conclusion", "conclusion", "conclusive", "conclusively"),
        ["put forward"] = ("put forward, propose", "proposal", "forward", "forward"),
        ["bring about"] = ("bring about, cause", "cause", null, null),
        ["set forth"] = ("set forth, state", "statement", null, "forth"),
        ["carry out"] = ("carry out, execute", "execution", "executable", null),
        ["adhere to"] = ("adhere, adhere to", "adherence, adhesive", "adherent, adhesive", "adhesively"),
        ["abide by"] = ("abide, abide by", "abidance", "abiding", "abidingly"),
        ["conform to"] = ("conform, conform to", "conformity, conformance", "conforming, conformable", "conformably"),
        ["amount to"] = ("amount, amount to", "amount", null, null),
        ["bring up"] = ("bring up, raise", "upbringing", null, null),
        ["follow up"] = ("follow up", "follow-up", "follow-up", null),
        ["sort out"] = ("sort out, sort", "sorting", "sorted", null),
        ["lay off"] = ("lay off", "layoff", "laid-off", null),
        ["consist of"] = ("consist, consist of", "consistency", "consistent", "consistently"),
        ["give rise to"] = ("give rise to, cause", "rise, cause", null, null),
        ["attributable to"] = ("attribute", "attribute, attribution", "attributable, attributable to", null),
        ["in light of"] = (null, "light", "in light of", null),
        ["on behalf of"] = (null, "behalf", null, "on behalf of"),
        ["by means of"] = ("mean", "means", null, "by means of"),
        ["in accordance with"] = ("accord", "accordance", "accordant", "in accordance with, accordingly"),
        ["for the sake of"] = (null, "sake", null, "for the sake of"),
        ["in the event of"] = (null, "event", "eventual", "in the event of, eventually"),
        ["on the verge of"] = ("verge", "verge", "on the verge of", null),
        ["in pursuit of"] = ("pursue", "pursuit", "in pursuit of", null),
        ["at the expense of"] = ("expend", "expense", "expensive", "at the expense of, expensively"),
        ["in lieu of"] = (null, "lieu", null, "in lieu of"),
        ["in excess of"] = ("exceed", "excess", "excessive, in excess of", "excessively"),
        ["take over"] = ("take over", "takeover", null, null),

        // Business Idioms
        ["touch base"] = ("touch base", "touch, contact", null, null),
        ["get the ball rolling"] = ("get the ball rolling, start", "start", null, null),
        ["think outside the box"] = ("think outside the box", "creativity", "creative", "creatively"),
        ["hit the ground running"] = ("hit the ground running", "readiness", "ready", null),
        ["in the loop"] = ("loop", "loop", "in the loop, informed", null),
        ["cut corners"] = ("cut corners", "shortcut, corner", null, null),
        ["go the extra mile"] = ("go the extra mile", "extra mile, effort", null, null),
        ["keep one's eye on the ball"] = ("keep one's eye on the ball, focus", "focus", "focused", null),
        ["raise the bar"] = ("raise the bar", "standard, benchmark", null, null),
        ["get down to business"] = ("get down to business", "business", null, null),
        ["a win-win situation"] = ("win", "win-win situation", "win-win", null),
        ["read between the lines"] = ("read between the lines", "insight", "insightful", null),
        ["put all one's eggs in one basket"] = ("put all one's eggs in one basket, risk", "risk", "risky", null),
        ["the bottom line"] = (null, "bottom line", "fundamental", "fundamentally"),
        ["ahead of the curve"] = (null, "curve", "ahead of the curve, advanced", "ahead"),
        ["behind the scenes"] = (null, "scene", "behind-the-scenes", "behind the scenes"),
        ["by the book"] = (null, "book", "by-the-book", "by the book, strictly"),
        ["get something off the ground"] = ("get off the ground, launch", "launch", null, null),
        ["in a nutshell"] = (null, "nutshell", "concise", "in a nutshell, concisely"),
        ["on the back burner"] = (null, "back burner", "delayed, deferred", "on the back burner"),
        ["pull one's weight"] = ("pull one's weight, cooperate", "cooperation", "cooperative", null),
        ["silver lining"] = (null, "silver lining", "optimistic, hopeful", "optimistically"),
        ["take it for granted"] = ("take for granted, assume", "assumption", null, null),
        ["throw in the towel"] = ("throw in the towel, surrender", "surrender", null, null),
        ["up in the air"] = (null, "uncertainty", "up in the air, uncertain", "uncertainly"),
        ["wild goose chase"] = ("chase", "wild goose chase", "futile", "futilely"),
        ["word of mouth"] = (null, "word of mouth", "word-of-mouth", "by word of mouth"),
        ["play it by ear"] = ("play it by ear, improvise", "improvisation", "improvised", null),
        ["bite the bullet"] = ("bite the bullet, endure", "endurance", null, null),
        ["burn the midnight oil"] = ("burn the midnight oil, overwork", "overwork", null, null),
        ["on thin ice"] = (null, "risk", "on thin ice, risky", "precariously"),
        ["a stone's throw"] = (null, "stone's throw", "nearby, close", "close by"),
        ["by leaps and bounds"] = ("leap, bound", "leap, bound", "rapid", "by leaps and bounds, rapidly"),
        ["get cold feet"] = ("get cold feet, hesitate", "hesitation", "hesitant", null),
        ["go out of one's way"] = ("go out of one's way", "effort", null, null),
        ["hit the roof"] = ("hit the roof, rage", "rage", "furious", "furiously"),
        ["keep an eye on"] = ("keep an eye on, monitor", "surveillance, eye", null, null),
        ["nip in the bud"] = ("nip in the bud, suppress", "suppression", null, null),
        ["rock the boat"] = ("rock the boat, disrupt", "disruption", "disruptive", null),
        ["sweep under the rug"] = ("sweep under the rug, conceal", "concealment", null, null),
        ["break the ice"] = ("break the ice", "icebreaker", null, null),

        // Numbers, Dates & Common Prepositional Forms
        ["as of"] = (null, null, null, "as of"),
        ["by the end of"] = ("end", "end", null, "by the end of"),
        ["prior to"] = (null, "priority", "prior", "prior to, previously"),

        // Health & Environment
        ["sustainability initiative"] = ("sustain, initiate", "sustainability initiative", "sustainable", "sustainably"),
        ["orientation seminar"] = ("orient", "orientation seminar", "oriented", null)
    };

    public static ToeicWordFamily GenerateFamily(string rawWord, string pos)
    {
        var word = rawWord.Trim();
        var lower = word.ToLowerInvariant();

        // 1. Direct Curated Match
        if (CuratedFamilies.TryGetValue(lower, out var curated))
        {
            return new ToeicWordFamily
            {
                V = curated.v,
                N = curated.n,
                Adj = curated.adj,
                Adv = curated.adv
            };
        }

        // 2. Strip standard affixes or common preposition suffixes (e.g. "dispose of" -> "dispose")
        var cleanWord = Regex.Replace(lower, @"\s+(of|with|to|in|for|from|on|at|by)$", "");
        if (CuratedFamilies.TryGetValue(cleanWord, out var curatedClean))
        {
            return new ToeicWordFamily
            {
                V = curatedClean.v,
                N = curatedClean.n,
                Adj = curatedClean.adj,
                Adv = curatedClean.adv
            };
        }

        // 3. Multi-word phrases: find operative headword in dictionary
        var tokens = lower.Split(new[] { ' ', '-' }, StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length > 1)
        {
            foreach (var token in tokens)
            {
                if (token.Length > 3 && CuratedFamilies.TryGetValue(token, out var tokenFamily))
                {
                    return new ToeicWordFamily
                    {
                        V = tokenFamily.v,
                        N = tokenFamily.n ?? word,
                        Adj = tokenFamily.adj,
                        Adv = tokenFamily.adv
                    };
                }
            }
        }

        // 4. Algorithmic Derivation Engine
        return DeriveAlgorithmicFamily(word, pos);
    }

    private static ToeicWordFamily DeriveAlgorithmicFamily(string word, string pos)
    {
        var lower = word.ToLowerInvariant();
        string? v = null;
        string? n = null;
        string? adj = null;
        string? adv = null;

        // Populate base slot based on POS
        if (pos.Contains("v")) v = lower;
        if (pos.Contains("n")) n = lower;
        if (pos.Contains("adj")) adj = lower;
        if (pos.Contains("adv")) adv = lower;

        // Derivation rules based on morphological endings
        if (lower.EndsWith("tion") || lower.EndsWith("sion"))
        {
            n = n ?? lower;
            var stem = lower.EndsWith("tion") ? lower[..^4] : lower[..^4];
            v = v ?? (stem.EndsWith("a") ? stem + "te" : stem.EndsWith("p") || stem.EndsWith("c") ? stem + "t" : stem + "e");
            adj = adj ?? (lower + "al");
            adv = adv ?? (adj + "ly");
        }
        else if (lower.EndsWith("ment"))
        {
            n = n ?? lower;
            v = v ?? lower[..^4];
            adj = adj ?? (v + "able");
            adv = adv ?? (adj + "ly");
        }
        else if (lower.EndsWith("ance") || lower.EndsWith("ence"))
        {
            n = n ?? lower;
            var stem = lower[..^4];
            v = v ?? (stem.EndsWith("i") ? stem[..^1] + "y" : stem + "e");
            adj = adj ?? (stem + (lower.EndsWith("ance") ? "ant" : "ent"));
            adv = adv ?? (adj + "ly");
        }
        else if (lower.EndsWith("ity"))
        {
            n = n ?? lower;
            var stem = lower[..^3];
            adj = adj ?? (stem.EndsWith("bil") ? stem[..^3] + "ble" : stem + "e");
            v = v ?? (stem.EndsWith("iv") ? stem[..^2] + "e" : null);
            adv = adv ?? (adj != null ? adj + "ly" : null);
        }
        else if (lower.EndsWith("able") || lower.EndsWith("ible"))
        {
            adj = adj ?? lower;
            var stem = lower[..^4];
            n = n ?? (stem + (lower.EndsWith("able") ? "ability" : "ibility"));
            v = v ?? (stem.EndsWith("i") ? stem[..^1] + "y" : stem + "e");
            adv = adv ?? (lower[..^1] + "y");
        }
        else if (lower.EndsWith("ive"))
        {
            adj = adj ?? lower;
            var stem = lower[..^3];
            n = n ?? (stem + "tion, " + lower + "ness");
            v = v ?? (stem.EndsWith("t") ? stem + "e" : stem);
            adv = adv ?? (lower + "ly");
        }
        else if (lower.EndsWith("ize") || lower.EndsWith("ise"))
        {
            v = v ?? lower;
            var stem = lower[..^3];
            n = n ?? (stem + "ization");
            adj = adj ?? (lower + "d");
        }
        else if (lower.EndsWith("ate") && lower.Length > 4)
        {
            v = v ?? lower;
            var stem = lower[..^1];
            n = n ?? (stem + "ion, " + stem + "or");
            adj = adj ?? (stem + "ive");
            adv = adv ?? (stem + "ively");
        }
        else if (lower.EndsWith("ful"))
        {
            adj = adj ?? lower;
            var stem = lower[..^3];
            n = n ?? stem;
            adv = adv ?? (lower + "ly");
        }
        else if (lower.EndsWith("less"))
        {
            adj = adj ?? lower;
            var stem = lower[..^4];
            n = n ?? (stem + ", " + lower + "ness");
            adv = adv ?? (lower + "ly");
        }
        else if (lower.EndsWith("ly") && lower.Length > 4)
        {
            adv = adv ?? lower;
            var stem = lower[..^2];
            adj = adj ?? stem;
            n = n ?? (stem + "ness");
        }
        else if (lower.EndsWith("er") || lower.EndsWith("or"))
        {
            n = n ?? lower;
            var stem = lower[..^2];
            v = v ?? (stem.EndsWith("at") ? stem + "e" : stem);
            adj = adj ?? (v + "able");
        }
        else
        {
            // Fallback: assign to its natural part of speech
            if (pos == "v")
            {
                v = v ?? lower;
                n = n ?? (lower.EndsWith("e") ? lower[..^1] + "ing, " + lower[..^1] + "ation" : lower + "ing, " + lower + "er");
                adj = adj ?? (lower.EndsWith("e") ? lower + "d" : lower + "ed");
            }
            else if (pos == "n")
            {
                n = n ?? lower;
                v = v ?? (lower.EndsWith("e") ? lower : lower);
                adj = adj ?? (lower.EndsWith("e") ? lower[..^1] + "al" : lower + "al");
                adv = adv ?? (adj != null ? adj + "ly" : null);
            }
            else if (pos == "adj")
            {
                adj = adj ?? lower;
                n = n ?? (lower + "ness");
                adv = adv ?? (lower.EndsWith("e") ? lower + "ly" : lower + "ly");
            }
        }

        // Guaranteed fallback so no word family is ever completely empty
        if (v == null && n == null && adj == null && adv == null)
        {
            if (pos == "prep" || pos == "adv") adv = lower;
            else if (pos == "adj") adj = lower;
            else if (pos == "v") v = lower;
            else n = lower;
        }

        // Clean duplicates
        return new ToeicWordFamily
        {
            V = CleanSlot(v),
            N = CleanSlot(n),
            Adj = CleanSlot(adj),
            Adv = CleanSlot(adv)
        };
    }

    private static string? CleanSlot(string? val)
    {
        if (string.IsNullOrWhiteSpace(val)) return null;
        var parts = val.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var cleaned = new List<string>();
        foreach (var p in parts)
        {
            var t = p.Trim();
            if (t.Length > 0 && seen.Add(t))
            {
                cleaned.Add(t);
            }
        }
        return cleaned.Count > 0 ? string.Join(", ", cleaned) : null;
    }

    public static void ProcessAllToeicWords(string jsonPath)
    {
        var text = File.ReadAllText(jsonPath);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        var data = JsonSerializer.Deserialize<ToeicData>(text, options);
        if (data == null) throw new InvalidOperationException("Failed to deserialize toeic.json");

        int processed = 0;
        foreach (var cat in data.VocabCategories)
        {
            foreach (var item in cat.Items)
            {
                item.WordFamily = GenerateFamily(item.En, item.Pos);
                processed++;
            }
        }

        var updatedJson = JsonSerializer.Serialize(data, options);
        File.WriteAllText(jsonPath, updatedJson, new System.Text.UTF8Encoding(false));
    }
}
